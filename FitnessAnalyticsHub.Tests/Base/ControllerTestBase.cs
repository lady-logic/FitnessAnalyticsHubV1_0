namespace FitnessAnalyticsHub.Tests.Base
{
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Base class for all controller tests that enforces architectural rules.
    /// Ensures controllers follow Clean Architecture principles.
    /// </summary>
    /// <typeparam name="TController">The controller type to test</typeparam>
    public abstract class ControllerTestBase<TController>
        where TController : ControllerBase
    {
        /// <summary>
        /// Verifies that the controller doesn't contain any try-catch blocks.
        /// Controllers should let exceptions bubble up to the middleware.
        /// </summary>
        [Fact]
        public void Controller_ShouldNotContainTryCatchBlocks()
        {
            // Arrange
            Type controllerType = typeof(TController);
            MethodInfo[] methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            // Act & Assert
            foreach (MethodInfo method in methods)
            {
                // Skip inherited methods from ControllerBase
                if (method.DeclaringType == typeof(ControllerBase) ||
                    method.DeclaringType == typeof(object))
                {
                    continue;
                }

                MethodBody? methodBody = method.GetMethodBody();
                if (methodBody != null)
                {
                    // Check for try-catch patterns in IL code
                    bool hasTryCatch = ContainsTryCatchBlock(method);

                    Assert.False(
                        hasTryCatch,
                        $"Controller method '{controllerType.Name}.{method.Name}' contains try-catch block. " +
                        "Controllers should not handle exceptions directly - let middleware handle them.");
                }
            }
        }

        /// <summary>
        /// Verifies that all controller action methods are async.
        /// This ensures consistent async patterns across the application.
        /// </summary>
        [Fact]
        public void Controller_ActionMethodsShouldBeAsync()
        {
            // Arrange
            Type controllerType = typeof(TController);
            IEnumerable<MethodInfo> actionMethods = GetActionMethods(controllerType);

            // Act & Assert
            foreach (MethodInfo method in actionMethods)
            {
                Assert.True(
                    method.ReturnType.IsAssignableFrom(typeof(Task)) ||
                           method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>),
                    $"Action method '{controllerType.Name}.{method.Name}' should be async and return Task or Task<T>");
            }
        }

        /// <summary>
        /// Verifies that all action methods have proper HTTP method attributes.
        /// </summary>
        [Fact]
        public void Controller_ActionMethodsShouldHaveHttpAttributes()
        {
            // Arrange
            Type controllerType = typeof(TController);
            IEnumerable<MethodInfo> actionMethods = GetActionMethods(controllerType);

            // Act & Assert
            foreach (MethodInfo method in actionMethods)
            {
                bool hasHttpAttribute = method.GetCustomAttributes()
                    .Any(attr => attr.GetType().Name.StartsWith("Http") &&
                                attr.GetType().Name.EndsWith("Attribute"));

                Assert.True(
                    hasHttpAttribute,
                    $"Action method '{controllerType.Name}.{method.Name}' should have an HTTP method attribute (HttpGet, HttpPost, etc.)");
            }
        }

        /// <summary>
        /// Detects try-catch blocks in method IL code.
        /// This is a simplified implementation - in production you might want a more sophisticated approach.
        /// </summary>
        private static bool ContainsTryCatchBlock(MethodInfo method)
        {
            try
            {
                MethodBody? methodBody = method.GetMethodBody();
                if (methodBody == null)
                {
                    return false;
                }

                // Check for exception handling clauses (try-catch blocks)
                IList<ExceptionHandlingClause> exceptionHandlingClauses = methodBody.ExceptionHandlingClauses;
                return exceptionHandlingClauses.Count > 0;
            }
            catch
            {
                // If we can't analyze the method, assume it's okay
                return false;
            }
        }

        /// <summary>
        /// Gets all action methods from the controller (excludes non-action methods).
        /// </summary>
        private static IEnumerable<MethodInfo> GetActionMethods(Type controllerType)
        {
            return controllerType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName && // Exclude properties, events, etc.
                           m.DeclaringType == controllerType && // Only declared in this controller
                           !m.GetCustomAttributes<NonActionAttribute>().Any()); // Not marked as NonAction
        }
    }
}