using AutoFixture;
using Domain.BaseObjectsNamespace;
using Domain.Entities.Requests;
using Domain.Entities.Requests.Events;
using Domain.Entities.Users;
using Domain.Entities.WorkflowTemplates;
using FluentAssertions;
using NUnit.Framework;

namespace DomainTests.Entities.Requests
{
    [TestFixture]
    class RequestTests
    {
        private Fixture _fixture;

        public RequestTests()
        {
            _fixture = new Fixture();
        }

        private (Request request, User user) CreateRequest()
        {
            var name = _fixture.Create<string>();
            var email = new Email(_fixture.Create<string>() + "@gmail.com");
            var role = new Role("TestRole");
            var password = new Password("Test@123");
            var document = new Document(email, name, "1234567890", DateTime.Now);
            var user = User.Create(name, email, role, password);
            List<WorkflowStepTemplate> steps = CreateDefaultSteps(user.Id, role.Id);
            WorkflowTemplate workflowTemplate = new WorkflowTemplate(Guid.NewGuid(), "HR", steps.ToArray());
            var request = workflowTemplate.CreateRequest(user, document);
            return (request, user);
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

        private void ProgressApprove(Request request, User user)
        {
            while (request.Progress.CurrentStep < request.Workflow.Steps.Count)
            {
                request.Approve(user);
            }
        }

        [Test]
        public void Restart_ShouldResetProgress_WhenCalledTest()
        {
            // Arrange
            var (request, user) = CreateRequest();

            // Act
            request.Restart();

            // Assert
            request.IsApproved().Should().BeFalse();
            request.IsRejected().Should().BeFalse();
            request.Progress.CurrentStep.Should().Be(0);
        }

        [Test]
        public void Approve_ShouldApproveRequest_WhenValidTest()
        {
            // Arrange
            var (request, user) = CreateRequest();

            // Act
            ProgressApprove(request, user);

            // Assert
            request.IsApproved().Should().BeTrue();
            request.IsRejected().Should().BeFalse();
            request.EventsList.Should().ContainSingle(e => e is RequestApprovedEvent);
        }

        [Test]
        public void Reject_ShouldRejectRequest_WhenValidTest()
        {
            // Arrange
            var (request, user) = CreateRequest();

            // Act
            request.Reject(user);

            // Assert
            request.IsRejected().Should().BeTrue();
            request.IsApproved().Should().BeFalse();
            request.EventsList.Should().ContainSingle(e => e is RequestRejectEvent);
        }

        [Test]
        public void EventsList_ShouldContainEvents_WhenActionsPerformedTest()
        {
            // Arrange
            var (request, user) = CreateRequest();

            // Act
            ProgressApprove(request, user);

            // Assert
            request.EventsList.Should().NotBeEmpty().And.ContainItemsAssignableTo<IEvent>();
        }

        [Test]
        public void Reject_ShouldThrowException_WhenAlreadyApprovedTest()
        {
            // Arrange
            var (request, user) = CreateRequest();
            
            ProgressApprove(request, user);

            // Act & Assert
            Action act = () => request.Reject(user);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Request is already approved or rejected");
        }

        [Test]
        public void Approve_ShouldThrowException_WhenInvalidUserTest()
        {
            // Arrange
            var (request, user) = CreateRequest();
            var invalidUser = new User(Guid.NewGuid(), "Invalid User", new Email("invaliduser@example.com"),
                Guid.NewGuid(), new Password("Test@123"));

            // Act
            Action act = () => request.Approve(invalidUser);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("User does not have permission to perform this action.");
        }

        [Test]
        public void Approve_ShouldAdvanceStepAndNotThrowException_WhenLastStepTest()
        {
            // Arrange
            var (request, user) = CreateRequest();
            while (request.Progress.CurrentStep < request.Workflow.Steps.Count - 1)
            {
                request.Approve(user);
            }

            // Act
            Action act = () => request.Approve(user);

            // Assert
            act.Should().NotThrow<InvalidOperationException>();
            request.IsApproved().Should().BeTrue();
        }

        [Test]
        public void Approve_ShouldNotAdvanceStep_WhenAlreadyApprovedTest()
        {
            // Arrange
            var (request, user) = CreateRequest();
            ProgressApprove(request, user);

            // Act
            Action act = () => request.Approve(user);

            // Assert
            act.Should().NotThrow<InvalidOperationException>();
            request.IsApproved().Should().BeTrue();
        }

        [Test]
        public void UserId_ShouldBeSet_WhenRequestCreated()
        {
            // Arrange
            var (request, user) = CreateRequest();

            // Assert
            request.UserId.Should().Be(user.Id);
        }
        
        [Test]
        public void Approve_ShouldThrowException_WhenAlreadyApprovedOrRejected()
        {
            // Arrange
            var (request, user) = CreateRequest();
            request.Reject(user);

            // Act & Assert
            Action act = () => request.Approve(user);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot advance to the next step: either the request is already approved or rejected, or there are no more steps available.");
        }
    }
}