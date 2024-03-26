using Application.Repositories;
using Application.Requests.Commands;
using Application.Requests.Handlers;
using AutoFixture;
using Domain.BaseObjectsNamespace;
using Domain.Entities.Requests;
using Domain.Entities.Users;
using Domain.Entities.WorkflowTemplates;
using Moq;
using NUnit.Framework;

namespace ApplicationTests;

[TestFixture]
public class CreateRequestHandlerTests
{
    private Fixture _fixture;

    public CreateRequestHandlerTests()
    {
        _fixture = new Fixture();
    }

    private static List<WorkflowStepTemplate> CreateDefaultSteps(Guid userId, Guid roleGuid)
    {
        return new List<WorkflowStepTemplate>
        {
            new WorkflowStepTemplate("Online Interview", 1, userId, roleGuid),
            new WorkflowStepTemplate("Interview with HR", 2, userId, roleGuid),
            new WorkflowStepTemplate("Technical Task", 3, userId, roleGuid),
            new WorkflowStepTemplate("Meeting with CEO", 4, userId, roleGuid),
        };
    }

    [Test]
    public void Handle_ValidCommand_RequestCreatedTest()
    {
        // Arrange
        var tenantFactoryMock = new Mock<ITenantFactory>();
        var tenantMock = new Mock<ITenant>();
        var requestRepositoryMock = new Mock<IRequestRepository>();
        var userRepositoryMock = new Mock<IUserRepository>();
        var workflowTemplateRepositoryMock = new Mock<IWorkflowTemplateRepository>();

        var name = _fixture.Create<string>();
        var email = new Email(_fixture.Create<string>() + "@gmail.com");
        var role = new Role("TestRole");
        var password = new Password("Test@123");
        var document = new Document(email, name, "1234567890", DateTime.Now);
        var user = User.Create(name, email, role, password);
        List<WorkflowStepTemplate> steps = CreateDefaultSteps(user.Id, role.Id);
        WorkflowTemplate workflowTemplate = new WorkflowTemplate(Guid.NewGuid(), "HR", steps.ToArray());
        var request = workflowTemplate.CreateRequest(user, document);

        tenantFactoryMock.Setup(factory => factory.GetTenant()).Returns(tenantMock.Object);
        tenantMock.Setup(tenant => tenant.Requests).Returns(requestRepositoryMock.Object);
        tenantMock.Setup(tenant => tenant.Users).Returns(userRepositoryMock.Object);
        tenantMock.Setup(tenant => tenant.WorkflowsTemplate).Returns(workflowTemplateRepositoryMock.Object);
        workflowTemplateRepositoryMock.Setup(repo => repo.GetById(workflowTemplate.Id)).Returns(workflowTemplate);
        userRepositoryMock.Setup(repo => repo.GetById(user.Id)).Returns(user);

        var handler = new CreateRequestHandler(tenantFactoryMock.Object);
        var command = new CreateRequestCommand(user.Id, document, workflowTemplate.Id);

        // Act
        handler.Handle(command);

        // Assert
        requestRepositoryMock.Verify(repo => repo.Add(It.IsAny<Request>()), Times.Once);
        tenantMock.Verify(tenant => tenant.Commit(), Times.Once);
    }
}