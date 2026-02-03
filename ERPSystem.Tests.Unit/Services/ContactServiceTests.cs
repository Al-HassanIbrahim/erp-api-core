using Moq;
using FluentAssertions;
using ERPSystem.Application.Services.Contacts;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Entities.Contacts;
using ERPSystem.Application.DTOs.Contacts;
using ERPSystem.Application.Exceptions;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Tests.Unit.Services
{
    public class ContactServiceTests
    {
        private readonly Mock<IContactRepository> _repoMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly Mock<IModuleAccessService> _moduleAccessMock;
        private readonly ContactService _service;
        private readonly int _companyId = 1;

        public ContactServiceTests()
        {
            _repoMock = new Mock<IContactRepository>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _moduleAccessMock = new Mock<IModuleAccessService>();

            _currentUserMock.Setup(x => x.CompanyId).Returns(_companyId);

            _service = new ContactService(
                _repoMock.Object,
                _currentUserMock.Object,
                _moduleAccessMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnContacts_WhenModuleIsEnabled()
        {
            // Given
            var cancellationToken = CancellationToken.None;
            var contacts = new List<Contact>
            {
                new Contact { Id = 1, FullName = "Contact 1", Company = "Co 1", Email = "c1@test.com", CompanyId = _companyId },
                new Contact { Id = 2, FullName = "Contact 2", Company = "Co 2", Email = "c2@test.com", CompanyId = _companyId }
            };

            _repoMock.Setup(r => r.GetAllContactsAsync(_companyId, cancellationToken))
                .ReturnsAsync(contacts);

            // When
            var result = await _service.GetAllAsync(cancellationToken);

            // Then
            result.Should().HaveCount(2);
            result[0].FullName.Should().Be("Contact 1");
            _moduleAccessMock.Verify(m => m.EnsureContactEnabledAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetContact_ShouldReturnContact_WhenExists()
        {
            // Given
            var contactId = 1;
            var cancellationToken = CancellationToken.None;
            var contact = new Contact { Id = contactId, FullName = "Contact 1", Company = "Co 1", Email = "c1@test.com", CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdAsync(contactId, _companyId, cancellationToken))
                .ReturnsAsync(contact);

            // When
            var result = await _service.GetContact(contactId, cancellationToken);

            // Then
            result.Should().NotBeNull();
            result!.FullName.Should().Be("Contact 1");
        }

        [Fact]
        public async Task GetContact_ShouldThrowException_WhenNotFound()
        {
            // Given
            var contactId = 1;
            var cancellationToken = CancellationToken.None;

            _repoMock.Setup(r => r.GetByIdAsync(contactId, _companyId, cancellationToken))
                .ReturnsAsync((Contact?)null);

            // When
            var act = () => _service.GetContact(contactId, cancellationToken);

            // Then
            await act.Should().ThrowAsync<BusinessException>()
                .Where(e => e.Code == "Contact_NOT_FOUND");
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedContact()
        {
            // Given
            var cancellationToken = CancellationToken.None;
            var request = new CreateContactRequest
            {
                FullName = "New Contact",
                Email = "new@test.com",
                Company = "New Co",
                Type = ContactPersonType.Client
            };

            // When
            var result = await _service.Create(request, cancellationToken);

            // Then
            result.FullName.Should().Be(request.FullName);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Contact>(), cancellationToken), Times.Once);
            _repoMock.Verify(r => r.SaveChanges(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldUpdateAndReturnContact_WhenExists()
        {
            // Given
            var contactId = 1;
            var cancellationToken = CancellationToken.None;
            var existingContact = new Contact { Id = contactId, FullName = "Old Name", Company = "Old Co", Email = "old@test.com", CompanyId = _companyId };
            var updateRequest = new UpdateContactDto
            {
                Id = contactId,
                FullName = "New Name",
                Email = "new@test.com",
                Company = "New Co",
                Type = ContactPersonType.Vendor
            };

            _repoMock.Setup(r => r.GetByIdAsync(contactId, _companyId, cancellationToken))
                .ReturnsAsync(existingContact);

            // When
            var result = await _service.Update(updateRequest, cancellationToken);

            // Then
            result.FullName.Should().Be("New Name");
            existingContact.FullName.Should().Be("New Name");
            _repoMock.Verify(r => r.Update(existingContact), Times.Once);
            _repoMock.Verify(r => r.SaveChanges(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldCallDeleteAndSave_WhenExists()
        {
            // Given
            var contactId = 1;
            var cancellationToken = CancellationToken.None;
            var existingContact = new Contact { Id = contactId, FullName = "Name", Company = "Co", Email = "e@test.com", CompanyId = _companyId };

            _repoMock.Setup(r => r.GetByIdAsync(contactId, _companyId, cancellationToken))
                .ReturnsAsync(existingContact);

            // When
            await _service.Delete(contactId, cancellationToken);

            // Then
            _repoMock.Verify(r => r.Delete(existingContact), Times.Once);
            _repoMock.Verify(r => r.SaveChanges(cancellationToken), Times.Once);
        }
    }
}
