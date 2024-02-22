using HR.LeaveManagement.Domain;
using HR.LeaveManagement.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace HR.LeaveManagement.Persistence.IntegrationTests
{
    public class HRDatabaseContextTests
    {
        private HRDatabaseContext _hrDatabaseContext;

        public HRDatabaseContextTests()
        {
            var dbOptions = new DbContextOptionsBuilder<HRDatabaseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            _hrDatabaseContext = new HRDatabaseContext(dbOptions);
        }

        [Fact]
        public async void Save_SetDateCreatedValue()
        {
            // Arrange
            var leaveType = new LeaveType
            {
                Id = 1,
                DefaultDays = 10,
                Name = "Test Vacation"
            };

            // Act

            _hrDatabaseContext.LeaveTypes.Add( leaveType );
            await _hrDatabaseContext.SaveChangesAsync();


            // Assert
            leaveType.DateCreated.ShouldNotBeNull();
        }
        [Fact]
        public async void Save_SetDateModifiedValue()
        {
            // Arrange
            var leaveType = new LeaveType
            {
                Id = 1,
                DefaultDays = 10,
                Name = "Test Vacation"
            };

            // Act

            _hrDatabaseContext.LeaveTypes.Add(leaveType);
            await _hrDatabaseContext.SaveChangesAsync();


            // Assert
            leaveType.DateModified.ShouldNotBeNull();
        }


    }
}
