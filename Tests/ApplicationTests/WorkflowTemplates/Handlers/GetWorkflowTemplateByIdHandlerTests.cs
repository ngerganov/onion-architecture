using Application.Repositories;
using Application.WorkflowTemplates.Handlers;
using Application.WorkflowTemplates.Queries;
using Domain.Entities.WorkflowTemplates;
using Moq;
using NUnit.Framework;

namespace ApplicationTests.WorkflowTemplates.Handlers;

[TestFixture]
public class GetWorkflowTemplateByIdHandlerTests
{
    [Test]
    public void Handle_ReturnsCorrectWorkflowTemplate_WhenValidIdIsProvidedTest()
    {
        // Arrange
        var tenantFactoryMock = new Mock<ITenantFactory>(MockBehavior.Strict);
        var tenantMock = new Mock<ITenant>(MockBehavior.Strict);
        var workflowRepositoryMock = new Mock<IWorkflowTemplateRepository>(MockBehavior.Strict);

        var handler = new GetWorkflowTemplateByIdHandler(tenantFactoryMock.Object);
        var query = new GetWorkflowTemplateByIdQuery(Guid.NewGuid());

        var expectedWorkflowTemplate = new WorkflowTemplate(Guid.NewGuid(), "Workflow 1", []);

        tenantFactoryMock.Setup(factory => factory.GetTenant()).Returns(tenantMock.Object);
        tenantMock.Setup(tenant => tenant.WorkflowsTemplate).Returns(workflowRepositoryMock.Object);
        workflowRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).Returns(expectedWorkflowTemplate);

        // Act
        var result = handler.Handle(query);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result, Is.EqualTo(expectedWorkflowTemplate));
    }
}