namespace FitnessAnalyticsHub.Tests.Exceptions
{
    using FitnessAnalyticsHub.Domain.Exceptions.Athletes;

    public class AthleteExceptionTests
    {
        [Fact]
        public void AthleteNotFoundException_ShouldSetCorrectMessage_WhenCreatedWithAthleteId()
        {
            // Arrange
            var athleteId = 123;

            // Act
            var exception = new AthleteNotFoundException(athleteId);

            // Assert
            Assert.Equal("Athlete with ID 123 not found", exception.Message);
            Assert.Equal(athleteId, exception.AthleteId);
        }

        [Fact]
        public void AthleteNotFoundException_ShouldInheritFromException()
        {
            // Arrange & Act
            var exception = new AthleteNotFoundException(1);

            // Assert
            Assert.IsAssignableFrom<Exception>(exception);
        }

        [Fact]
        public void AthleteNotFoundException_ShouldHaveAthleteIdProperty()
        {
            // Arrange
            var athleteId = 456;

            // Act
            var exception = new AthleteNotFoundException(athleteId);

            // Assert
            Assert.Equal(athleteId, exception.AthleteId);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(999)]
        [InlineData(-1)]
        public void AthleteNotFoundException_ShouldWorkWithDifferentAthleteIds(int athleteId)
        {
            // Act
            var exception = new AthleteNotFoundException(athleteId);

            // Assert
            Assert.Equal(athleteId, exception.AthleteId);
            Assert.Contains(athleteId.ToString(), exception.Message);
        }
    }
}