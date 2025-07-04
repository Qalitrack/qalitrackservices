using AutoMapper;
using CustomerService.Core.Entities;
using CustomerService.Core.Interfaces;
using CustomerService.Tests.Helpers;
using System.Linq.Expressions;

namespace CustomerService.Tests.Helpers;

public static class MockFactories
{
    public static Mock<ICustomerService> CreateMockCustomerService()
    {
        var mock = new Mock<ICustomerService>();

        // Default setup for common scenarios
        var testCustomers = TestDataFactory.CreateTestCustomerList(5);
        var testCustomerDtos = testCustomers.Select(c => TestDataFactory.CreateTestCustomerDto(
            c.Id, c.Name, c.ContactEmail, c.CustomerType, c.Status)).ToList();

        mock.Setup(x => x.GetAllCustomersAsync())
            .ReturnsAsync(testCustomerDtos);

        mock.Setup(x => x.GetCustomersPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((int page, int pageSize) => 
                testCustomerDtos.Skip((page - 1) * pageSize).Take(pageSize));

        mock.Setup(x => x.SearchCustomersAsync(It.IsAny<string>()))
            .ReturnsAsync((string searchTerm) =>
                testCustomerDtos.Where(c => 
                    c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.ContactEmail.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));

        return mock;
    }

    public static Mock<ICustomerRepository> CreateMockCustomerRepository()
    {
        var mock = new Mock<ICustomerRepository>();
        var testCustomers = TestDataFactory.CreateTestCustomerList(5);

        // Setup basic CRUD operations
        mock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(testCustomers);

        mock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => testCustomers.FirstOrDefault(c => c.Id == id));

        mock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((string email) => testCustomers.FirstOrDefault(c => c.ContactEmail == email));

        mock.Setup(x => x.GetByTaxNumberAsync(It.IsAny<string>()))
            .ReturnsAsync((string taxNumber) => testCustomers.FirstOrDefault(c => c.TaxNumber == taxNumber));

        mock.Setup(x => x.GetByRegistrationNumberAsync(It.IsAny<string>()))
            .ReturnsAsync((string regNumber) => testCustomers.FirstOrDefault(c => c.RegistrationNumber == regNumber));

        mock.Setup(x => x.AddAsync(It.IsAny<Customer>()))
            .ReturnsAsync((Customer customer) =>
            {
                customer.Id = Guid.NewGuid().ToString();
                customer.CreatedAt = DateTime.UtcNow;
                customer.UpdatedAt = DateTime.UtcNow;
                return customer;
            });

        mock.Setup(x => x.UpdateAsync(It.IsAny<Customer>()))
            .ReturnsAsync((Customer customer) =>
            {
                customer.UpdatedAt = DateTime.UtcNow;
                return customer;
            });

        mock.Setup(x => x.DeleteByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        mock.Setup(x => x.SearchAsync(It.IsAny<string>()))
            .ReturnsAsync((string searchTerm) =>
                testCustomers.Where(c =>
                    c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (c.TaxNumber?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.RegistrationNumber?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    c.ContactEmail.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));

        return mock;
    }

    public static Mock<IRepository<T>> CreateMockRepository<T>() where T : BaseEntity
    {
        var mock = new Mock<IRepository<T>>();

        mock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<T>());

        mock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((T?)null);

        mock.Setup(x => x.AddAsync(It.IsAny<T>()))
            .ReturnsAsync((T entity) =>
            {
                entity.Id = Guid.NewGuid().ToString();
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                return entity;
            });

        mock.Setup(x => x.UpdateAsync(It.IsAny<T>()))
            .ReturnsAsync((T entity) =>
            {
                entity.UpdatedAt = DateTime.UtcNow;
                return entity;
            });

        mock.Setup(x => x.DeleteByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        mock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<T, bool>>>()))
            .ReturnsAsync(new List<T>());

        mock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<T, bool>>>()))
            .ReturnsAsync((T?)null);

        return mock;
    }

    public static Mock<IMapper> CreateMockMapper()
    {
        var mock = new Mock<IMapper>();

        // Setup mapping for Customer to CustomerDto
        mock.Setup(x => x.Map<CustomerDto>(It.IsAny<Customer>()))
            .Returns((Customer source) => source == null ? null : TestDataFactory.CreateTestCustomerDto(
                source.Id, source.Name, source.ContactEmail, source.CustomerType, source.Status));

        mock.Setup(x => x.Map<IEnumerable<CustomerDto>>(It.IsAny<IEnumerable<Customer>>()))
            .Returns((IEnumerable<Customer> source) => source?.Select(c => TestDataFactory.CreateTestCustomerDto(
                c.Id, c.Name, c.ContactEmail, c.CustomerType, c.Status)) ?? new List<CustomerDto>());

        // Setup mapping for RegisterCustomerRequest to Customer
        mock.Setup(x => x.Map<Customer>(It.IsAny<RegisterCustomerRequest>()))
            .Returns((RegisterCustomerRequest source) => source == null ? null : TestDataFactory.CreateTestCustomer(
                name: source.Name, email: source.ContactEmail, customerType: source.CustomerType, creditLimit: source.CreditLimit));

        // Setup mapping for UpdateCustomerRequest to existing Customer
        mock.Setup(x => x.Map(It.IsAny<UpdateCustomerRequest>(), It.IsAny<Customer>()))
            .Callback((UpdateCustomerRequest source, Customer destination) =>
            {
                if (source != null && destination != null)
                {
                    destination.Name = source.Name;
                    destination.ContactEmail = source.ContactEmail;
                    destination.TaxNumber = source.TaxNumber;
                    destination.RegistrationNumber = source.RegistrationNumber;
                    destination.ContactPhone = source.ContactPhone;
                    destination.BillingAddress = source.BillingAddress;
                    destination.CustomerType = source.CustomerType;
                    destination.CreditLimit = source.CreditLimit;
                    destination.Status = source.Status;
                    destination.Notes = source.Notes;
                    destination.UpdatedAt = DateTime.UtcNow;
                }
            });

        // Setup mapping for Contact entities
        mock.Setup(x => x.Map<CustomerContact>(It.IsAny<CreateCustomerContactRequest>()))
            .Returns((CreateCustomerContactRequest source) => source == null ? null : 
                TestDataFactory.CreateTestCustomerContact());

        mock.Setup(x => x.Map<CustomerContactDto>(It.IsAny<CustomerContact>()))
            .Returns((CustomerContact source) => source == null ? null : new CustomerContactDto
            {
                Id = source.Id,
                CustomerId = source.CustomerId,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Email = source.Email,
                Phone = source.Phone,
                Position = source.Position,
                Department = source.Department,
                IsActive = source.IsActive
            });

        // Setup mapping for Contract entities
        mock.Setup(x => x.Map<CustomerContract>(It.IsAny<CreateCustomerContractRequest>()))
            .Returns((CreateCustomerContractRequest source) => source == null ? null :
                TestDataFactory.CreateTestCustomerContract());

        mock.Setup(x => x.Map<CustomerContractDto>(It.IsAny<CustomerContract>()))
            .Returns((CustomerContract source) => source == null ? null : new CustomerContractDto
            {
                Id = source.Id,
                CustomerId = source.CustomerId,
                ContractNumber = source.ContractNumber,
                Title = source.Title,
                Description = source.Description,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                ContractValue = source.ContractValue,
                Status = source.Status,
                ContractType = source.ContractType,
                Terms = source.Terms,
                AutoRenew = source.AutoRenew,
                RenewalPeriodMonths = source.RenewalPeriodMonths
            });

        // Setup mapping for Billing entities
        mock.Setup(x => x.Map<CustomerBillingDto>(It.IsAny<CustomerBilling>()))
            .Returns((CustomerBilling source) => source == null ? null : new CustomerBillingDto
            {
                Id = source.Id,
                CustomerId = source.CustomerId,
                BillingAddress = source.BillingAddress,
                BillingContactName = source.BillingContactName,
                BillingEmail = source.BillingEmail,
                BillingPhone = source.BillingPhone,
                BillingCity = source.BillingCity,
                BillingState = source.BillingState,
                BillingCountry = source.BillingCountry,
                BillingPostalCode = source.BillingPostalCode,
                PaymentTerms = source.PaymentTerms,
                PreferredPaymentMethod = source.PreferredPaymentMethod,
                TaxExemptNumber = source.TaxExemptNumber,
                IsTaxExempt = source.IsTaxExempt,
                Currency = source.Currency,
                DiscountPercentage = source.DiscountPercentage,
                AutomaticBilling = source.AutomaticBilling,
                BillingCycleDay = source.BillingCycleDay
            });

        // Setup mapping for Credit entities
        mock.Setup(x => x.Map<CustomerCreditDto>(It.IsAny<CustomerCredit>()))
            .Returns((CustomerCredit source) => source == null ? null : new CustomerCreditDto
            {
                Id = source.Id,
                CustomerId = source.CustomerId,
                CreditLimit = source.CreditLimit,
                AvailableCredit = source.AvailableCredit,
                UsedCredit = source.UsedCredit,
                CreditStatus = source.CreditStatus,
                CreditScore = source.CreditScore,
                LastCreditCheck = source.LastCreditCheck,
                NextCreditReview = source.NextCreditReview,
                CreditTerms = source.CreditTerms,
                PaymentHistory = new PaymentHistoryDto
                {
                    TotalInvoices = source.PaymentHistory.TotalInvoices,
                    OnTimePayments = source.PaymentHistory.OnTimePayments,
                    LatePayments = source.PaymentHistory.LatePayments,
                    TotalPaid = source.PaymentHistory.TotalPaid,
                    OutstandingBalance = source.PaymentHistory.OutstandingBalance,
                    LastPaymentDate = source.PaymentHistory.LastPaymentDate,
                    LastPaymentAmount = source.PaymentHistory.LastPaymentAmount,
                    AverageDaysToPayment = source.PaymentHistory.AverageDaysToPayment,
                    OldestOutstandingInvoice = source.PaymentHistory.OldestOutstandingInvoice
                },
                RequiresApproval = source.RequiresApproval,
                SecurityDeposit = source.SecurityDeposit,
                CreditReference1 = source.CreditReference1,
                CreditReference2 = source.CreditReference2,
                CreditReference3 = source.CreditReference3,
                Notes = source.Notes
            });

        return mock;
    }

    public static void SetupRepositoryForSuccessfulOperations<T>(Mock<IRepository<T>> mockRepo, List<T> data) 
        where T : BaseEntity
    {
        mockRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(data);
        mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => data.FirstOrDefault(item => item.Id == id));
        
        mockRepo.Setup(x => x.AddAsync(It.IsAny<T>()))
                .ReturnsAsync((T entity) =>
                {
                    entity.Id = Guid.NewGuid().ToString();
                    entity.CreatedAt = DateTime.UtcNow;
                    entity.UpdatedAt = DateTime.UtcNow;
                    data.Add(entity);
                    return entity;
                });
        
        mockRepo.Setup(x => x.UpdateAsync(It.IsAny<T>()))
                .ReturnsAsync((T entity) =>
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                    return entity;
                });
        
        mockRepo.Setup(x => x.DeleteByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) =>
                {
                    var item = data.FirstOrDefault(x => x.Id == id);
                    return item != null && data.Remove(item);
                });
    }

    public static void SetupRepositoryForFailureScenarios<T>(Mock<IRepository<T>> mockRepo) 
        where T : BaseEntity
    {
        mockRepo.Setup(x => x.AddAsync(It.IsAny<T>()))
                .ThrowsAsync(new InvalidOperationException("Database error occurred"));
        
        mockRepo.Setup(x => x.UpdateAsync(It.IsAny<T>()))
                .ThrowsAsync(new InvalidOperationException("Database error occurred"));
        
        mockRepo.Setup(x => x.DeleteByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
    }
}