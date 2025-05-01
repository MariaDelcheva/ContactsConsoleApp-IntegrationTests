using ContactsConsoleAPI.Business;
using ContactsConsoleAPI.Business.Contracts;
using ContactsConsoleAPI.Data.Models;
using ContactsConsoleAPI.DataAccess;
using ContactsConsoleAPI.DataAccess.Contrackts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsConsoleAPI.IntegrationTests.NUnit
{
    public class IntegrationTests
    {
        private TestContactDbContext dbContext;
        private IContactManager contactManager;

        [SetUp]
        public void SetUp()
        {
            this.dbContext = new TestContactDbContext();
            this.contactManager = new ContactManager(new ContactRepository(this.dbContext));
        }


        [TearDown]
        public void TearDown()
        {
            this.dbContext.Database.EnsureDeleted();
            this.dbContext.Dispose();
        }


        //positive test
        [Test]
        public async Task AddContactAsync_ShouldAddNewContact()
        {
            //Arrange
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(newContact);

          //Act

           var dbContact = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Contact_ULID == newContact.Contact_ULID);

             //Assert
            Assert.NotNull(dbContact);
            Assert.AreEqual(newContact.FirstName, dbContact.FirstName);
            Assert.AreEqual(newContact.LastName, dbContact.LastName);
            Assert.AreEqual(newContact.Phone, dbContact.Phone);
            Assert.AreEqual(newContact.Email, dbContact.Email);
            Assert.AreEqual(newContact.Address, dbContact.Address);
            Assert.AreEqual(newContact.Contact_ULID, dbContact.Contact_ULID);
        }

        //Negative test
        [Test]
        public async Task AddContactAsync_TryToAddContactWithInvalidCredentials_ShouldThrowException()
        {    
            //Arrange

            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "invalid_Mail", //invalid email
                Gender = "Male",
                Phone = "0889933779"
            };
           

                //Act && Assert

            var exception = Assert.ThrowsAsync<ValidationException>(async () => await contactManager.AddAsync(newContact));

            var result = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Contact_ULID == newContact.Contact_ULID);

            Assert.IsNull(result);
            Assert.That(exception.Message, Is.EqualTo("Invalid contact!"));

        }

        [Test]
        public async Task DeleteContactAsync_WithValidULID_ShouldRemoveContactFromDb()
        {
            //Arrange
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(newContact);


            // Act
            await contactManager.DeleteAsync(newContact.Contact_ULID);

            // Assert
         var contactInDb = await dbContext.Contacts.FirstOrDefaultAsync(x => x.Contact_ULID == newContact.Contact_ULID);

            Assert.IsNull(contactInDb);

        }


        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]

        public async Task DeleteContactAsync_TryToDeleteWithNullOrWhiteSpaceULID_ShouldThrowException(string invalidUlid)
        {

            // Act &&  Assert

            var exception = Assert.ThrowsAsync<ArgumentException>(() => contactManager.DeleteAsync(invalidUlid));

            Assert.That(exception.Message, Is.EqualTo("ULID cannot be empty."));

        }

        [Test]
        public async Task GetAllAsync_WhenContactsExist_ShouldReturnAllContacts()
        {
            //Arrange
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(newContact);

         
            var newContact2 = new Contact()
            {
                FirstName = "TestSecondName",
                LastName = "SecondTestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(newContact2);

            // Act
            var result = await contactManager.GetAllAsync();


            // Assert
            Assert.That(result.Count, Is.EqualTo(2));

            var firstContact = result.First();
            Assert.That(firstContact.FirstName, Is.EqualTo(newContact.FirstName));
            Assert.That(firstContact.LastName, Is.EqualTo(newContact.LastName));
            Assert.That(firstContact.Address, Is.EqualTo(newContact.Address));
            Assert.That(firstContact.Contact_ULID, Is.EqualTo(newContact.Contact_ULID));
            Assert.That(firstContact.Email, Is.EqualTo(newContact.Email));
            Assert.That(firstContact.Gender, Is.EqualTo(newContact.Gender));
            Assert.That(firstContact.Phone, Is.EqualTo(newContact.Phone));
           

        }

        [Test]
        public async Task GetAllAsync_WhenNoContactsExist_ShouldThrowKeyNotFoundException()
        { 

            // Act &&  Assert
            
         var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.GetAllAsync());

            Assert.That(exception.Message, Is.EqualTo("No contact found."));
           
        }

        [Test]
        public async Task SearchByFirstNameAsync_WithExistingFirstName_ShouldReturnMatchingContacts()
        {
            //Arrange
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(newContact);

            // Act

            var result = await contactManager.SearchByFirstNameAsync(newContact.FirstName);

            // Assert
            Assert.IsNotNull(result);

            var contactInDb = result.First();
            Assert.That(contactInDb.FirstName, Is.EqualTo(newContact.FirstName));
            Assert.That(contactInDb.LastName, Is.EqualTo(newContact.LastName));
            Assert.That(contactInDb.Address, Is.EqualTo(newContact.Address));
            Assert.That(contactInDb.Contact_ULID, Is.EqualTo(newContact.Contact_ULID));
            Assert.That(contactInDb.Email, Is.EqualTo(newContact.Email));
            Assert.That(contactInDb.Gender, Is.EqualTo(newContact.Gender));
            Assert.That(contactInDb.Phone, Is.EqualTo(newContact.Phone));
        }

        [Test]
        public async Task SearchByFirstNameAsync_WithNonExistingFirstName_ShouldThrowKeyNotFoundException()
        {

            // Act &&  Assert

            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.SearchByFirstNameAsync("NON-EXISTING-NAME"));

            Assert.That(exception.Message, Is.EqualTo("No contact found with the given first name.")) ; 
        }

        [Test]
        public async Task SearchByLastNameAsync_WithExistingLastName_ShouldReturnMatchingContacts()
        {
            //Arrange
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(newContact);
          

            var newContact2 = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(newContact2);

            // Act
            var result = contactManager.SearchByLastNameAsync(newContact.LastName);

            // Assert
            Assert.IsNotNull(result);

            var contactInDb = await dbContext.Contacts.FirstOrDefaultAsync(x => x.LastName == newContact.LastName);

            Assert.That(contactInDb.FirstName, Is.EqualTo(newContact.FirstName));
            Assert.That(contactInDb.LastName, Is.EqualTo(newContact.LastName));
            Assert.That(contactInDb.Address, Is.EqualTo(newContact.Address));
            Assert.That(contactInDb.Contact_ULID, Is.EqualTo(newContact.Contact_ULID));
            Assert.That(contactInDb.Email, Is.EqualTo(newContact.Email));
            Assert.That(contactInDb.Gender, Is.EqualTo(newContact.Gender));
            Assert.That(contactInDb.Phone, Is.EqualTo(newContact.Phone));
        }

        [Test]
        public async Task SearchByLastNameAsync_WithNonExistingLastName_ShouldThrowKeyNotFoundException()
        {
            // Act && Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.SearchByLastNameAsync("Non-existing-lastName"));

            Assert.That(exception.Message, Is.EqualTo("No contact found with the given last name."));
        }

        [Test]
        public async Task GetSpecificAsync_WithValidULID_ShouldReturnContact()
        {
            // Arrange
           
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(newContact);

            var newContact2 = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(newContact2);

            // Act
            var result = await contactManager.GetSpecificAsync(newContact.Contact_ULID);

            // Assert
            Assert.IsNotNull(result);

            Assert.That(result.FirstName, Is.EqualTo(newContact.FirstName));
            Assert.That(result.LastName, Is.EqualTo(newContact.LastName));
            Assert.That(result.Address, Is.EqualTo(newContact.Address));
            Assert.That(result.Contact_ULID, Is.EqualTo(newContact.Contact_ULID));
            Assert.That(result.Email, Is.EqualTo(newContact.Email));
            Assert.That(result.Gender, Is.EqualTo(newContact.Gender));
            Assert.That(result.Phone, Is.EqualTo(newContact.Phone));

        }

        [Test]
        public async Task GetSpecificAsync_WithInvalidULID_ShouldThrowKeyNotFoundException()
        {
            const  string invalid_ULID = "Invalid_ULID62126";
            // Act  && Assert
            

            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.GetSpecificAsync(invalid_ULID));

            Assert.That(exception.Message, Is.EqualTo($"No contact found with ULID: {invalid_ULID}"));
        }

        [Test]
        public async Task UpdateAsync_WithValidContact_ShouldUpdateContact()
        {
            // Arrange
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(newContact);

            var newContact2 = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(newContact2);

            var updatedContact = newContact;
            updatedContact.FirstName = "UPDATED NAME";

            // Act

          await contactManager.UpdateAsync(newContact);

            // Assert

            Assert.IsNotNull(updatedContact);

            Assert.That(updatedContact.FirstName,Is.EqualTo(newContact.FirstName));
            Assert.That(updatedContact.LastName, Is.EqualTo(newContact.LastName));
            Assert.That(updatedContact.Address, Is.EqualTo(newContact.Address));
            Assert.That(updatedContact.Contact_ULID, Is.EqualTo(newContact.Contact_ULID));
            Assert.That(updatedContact.Email, Is.EqualTo(newContact.Email));
            Assert.That(updatedContact.Gender, Is.EqualTo(newContact.Gender));
            Assert.That(updatedContact.Phone, Is.EqualTo(newContact.Phone));
        }

        [Test]
        public async Task UpdateAsync_WithInvalidContact_ShouldThrowValidationException()
        {


            // Act && Assert

            var exception = Assert.ThrowsAsync<ValidationException>(() => contactManager.UpdateAsync(new Contact() ));

            Assert.That(exception.Message, Is.EqualTo("Invalid contact!"));    

        }
    }
}
