namespace FitnessAnalyticsHub.Tests.Exceptions
{
    using FitnessAnalyticsHub.Domain.Exceptions.Activities;

    public class ActivityExceptionTests
    {
        [Fact]
        public void ActivityNotFoundException_ShouldSetCorrectMessage_WhenCreatedWithActivityId()
        {
            // Arrange
            var activityId = 789;

            // Act
            var exception = new ActivityNotFoundException(activityId);

            // Assert
            Assert.Equal("Activity with ID 789 not found", exception.Message);
            Assert.Equal(activityId, exception.ActivityId);
        }

        [Fact]
        public void ActivityNotFoundException_ShouldInheritFromException()
        {
            // Arrange & Act
            var exception = new ActivityNotFoundException(1);

            // Assert
            Assert.IsAssignableFrom<Exception>(exception);
        }

        [Fact]
        public void ActivityNotFoundException_ShouldHaveActivityIdProperty()
        {
            // Arrange
            var activityId = 654;

            // Act
            var exception = new ActivityNotFoundException(activityId);

            // Assert
            Assert.Equal(activityId, exception.ActivityId);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(12345)]
        [InlineData(0)]
        public void ActivityNotFoundException_ShouldWorkWithDifferentActivityIds(int activityId)
        {
            // Act
            var exception = new ActivityNotFoundException(activityId);

            // Assert
            Assert.Equal(activityId, exception.ActivityId);
            Assert.Contains(activityId.ToString(), exception.Message);
        }
    }
}