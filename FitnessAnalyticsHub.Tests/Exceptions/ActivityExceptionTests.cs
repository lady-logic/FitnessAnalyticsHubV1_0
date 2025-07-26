namespace FitnessAnalyticsHub.Tests.Exceptions
{
    using FitnessAnalyticsHub.Domain.Exceptions.Activities;

    public class ActivityExceptionTests
    {
        [Fact]
        public void ActivityNotFoundException_ShouldSetCorrectMessage_WhenCreatedWithActivityId()
        {
            // Arrange
            int activityId = 789;

            // Act
            ActivityNotFoundException exception = new ActivityNotFoundException(activityId);

            // Assert
            Assert.Equal("Activity with ID 789 not found", exception.Message);
            Assert.Equal(activityId, exception.ActivityId);
        }

        [Fact]
        public void ActivityNotFoundException_ShouldInheritFromException()
        {
            // Arrange & Act
            ActivityNotFoundException exception = new ActivityNotFoundException(1);

            // Assert
            Assert.IsAssignableFrom<Exception>(exception);
        }

        [Fact]
        public void ActivityNotFoundException_ShouldHaveActivityIdProperty()
        {
            // Arrange
            int activityId = 654;

            // Act
            ActivityNotFoundException exception = new ActivityNotFoundException(activityId);

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
            ActivityNotFoundException exception = new ActivityNotFoundException(activityId);

            // Assert
            Assert.Equal(activityId, exception.ActivityId);
            Assert.Contains(activityId.ToString(), exception.Message);
        }
    }
}