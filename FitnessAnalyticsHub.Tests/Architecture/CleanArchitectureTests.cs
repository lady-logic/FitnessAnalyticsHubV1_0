using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace FitnessAnalyticsHub.Tests.Architecture;

[Trait("Category", "Architecture")]
public class CleanArchitectureTests
{
    private static readonly ArchUnitNET.Domain.Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            System.Reflection.Assembly.Load("FitnessAnalyticsHub.Domain"),
            System.Reflection.Assembly.Load("FitnessAnalyticsHub.Application"),
            System.Reflection.Assembly.Load("FitnessAnalyticsHub.Infrastructure"),
            System.Reflection.Assembly.Load("FitnessAnalyticsHub.WebApi")
        )
        .Build();

    private readonly IObjectProvider<IType> DomainLayer =
        Types().That().ResideInAssembly("FitnessAnalyticsHub.Domain").As("Domain Layer");

    private readonly IObjectProvider<IType> ApplicationLayer =
        Types().That().ResideInAssembly("FitnessAnalyticsHub.Application").As("Application Layer");

    private readonly IObjectProvider<IType> InfrastructureLayer =
        Types().That().ResideInAssembly("FitnessAnalyticsHub.Infrastructure").As("Infrastructure Layer");

    private readonly IObjectProvider<IType> PresentationLayer =
        Types().That().ResideInAssembly("FitnessAnalyticsHub.WebApi").As("Presentation Layer");

    [Fact]
    public void DomainLayerShouldNotDependOnOtherLayers()
    {
        // In Clean Architecture sollte die Domain-Schicht von nichts abhängen
        // außer von sich selbst
        IArchRule rule = Types().That().Are(DomainLayer)
            .Should().OnlyDependOnTypesThat().Are(DomainLayer)
            .OrShould().OnlyDependOnTypesThat().ResideInNamespace("System")
            .OrShould().OnlyDependOnTypesThat().ResideInNamespace("Microsoft.Extensions")
            .As("Domain Layer sollte nur von sich selbst und Basisbibliotheken abhängen")
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    [Fact]
    public void ApplicationLayerShouldOnlyDependOnDomainLayer()
    {
        // Application Layer darf nur von Domain Layer und sich selbst abhängen
        IArchRule rule = Types().That().Are(ApplicationLayer)
            .Should().OnlyDependOnTypesThat().Are(DomainLayer)
            .OrShould().OnlyDependOnTypesThat().Are(ApplicationLayer)
            .OrShould().OnlyDependOnTypesThat().ResideInNamespace("System")
            .OrShould().OnlyDependOnTypesThat().ResideInNamespace("Microsoft.Extensions")
            .As("Application Layer sollte nur von Domain Layer, sich selbst und Basisbibliotheken abhängen")
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    [Fact]
    public void PresentationLayerShouldNotBeAccessedByAnyLayer()
    {
        // Keine andere Schicht sollte auf die Präsentationsschicht zugreifen
        IArchRule rule = Types().That().Are(PresentationLayer)
            .Should().NotDependOnAny(DomainLayer)
            .AndShould().NotDependOnAny(ApplicationLayer)
            .AndShould().NotDependOnAny(InfrastructureLayer)
            .As("Presentation Layer sollte von keiner anderen Schicht aufgerufen werden")
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    [Fact]
    public void InfrastructureLayerShouldNotBeAccessedByDomainOrApplicationLayer()
    {
        // Domain und Application sollten nicht direkt auf Infrastructure zugreifen
        IArchRule rule = Types().That().Are(InfrastructureLayer)
            .Should().NotDependOnAny(DomainLayer)
            .AndShould().NotDependOnAny(ApplicationLayer)
            .As("Infrastructure Layer sollte von Domain oder Application Layer nicht aufgerufen werden")
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    //[Fact]
    //public void EntitiesShouldResideInDomainLayer()
    //{
    //    // Annahme: Ihre Entitäten enden mit "Entity" oder implementieren eine IEntity-Schnittstelle
    //    var entities = Classes().That().HaveNameEndingWith("Entity")
    //        .Or().ImplementInterface(typeof(IEntity).FullName)
    //        .As("Entities");

    //    IArchRule rule = Classes().That().Are(entities)
    //        .Should().ResideInAssembly("FitnessAnalyticsHub.Domain")
    //        .As("Entities müssen in der Domain-Schicht definiert sein");

    //    rule.Check(Architecture);
    //}

    [Fact]
    public void RepositoriesShouldImplementCorrectInterfaces()
    {
        // Prüft, ob alle Repository-Implementierungen ihr Interface implementieren
        var repositoryClasses = Classes().That().HaveNameEndingWith("Repository")
            .And().DoNotHaveNameEndingWith("Interface")
            .As("Repository Classes");

        IArchRule rule = Classes().That().Are(repositoryClasses)
            .Should().ImplementInterface("IRepository`1") // Generischer Typ IRepository<T> wird als IRepository`1 notiert
            .As("Repository-Klassen müssen das IRepository-Interface implementieren")
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    [Fact]
    public void UseCasesShouldBeInApplicationLayer()
    {
        // Annahme: Ihre UseCases oder Handlers enden mit "UseCase", "Handler" oder "Service"
        var useCases = Classes().That()
            .HaveNameEndingWith("Service")
            .And().ResideInNamespace("FitnessAnalyticsHub.Application")
            .As("Use Cases");

        IArchRule rule = Classes().That().Are(useCases)
            .Should().ResideInNamespace("FitnessAnalyticsHub.Application")
            .As("Application-Services müssen im Application-Namespace sein")
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }
}
