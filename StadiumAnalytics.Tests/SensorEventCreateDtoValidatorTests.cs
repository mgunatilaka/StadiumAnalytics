using FluentValidation.TestHelper;
using StadiumAnalytics.Application.DTOs;
using StadiumAnalytics.Application.Validators;
using System;
using Xunit;

namespace StadiumAnalytics.Application.Tests.Validators
{
    public class SensorEventCreateDtoValidatorTests
    {
        private readonly SensorEventCreateDtoValidator _validator = new SensorEventCreateDtoValidator();

        [Fact]
        public void Should_Pass_Validation_For_Valid_Dto()
        {
            var dto = new SensorEventCreateDto
            {
                Gate = "Main Gate",
                NumberOfPeople = 10,
                Type = "Enter",
                Timestamp = DateTime.UtcNow
            };

            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_Gate_Is_Empty()
        {
            var dto = new SensorEventCreateDto
            {
                Gate = "",
                NumberOfPeople = 10,
                Type = "Enter",
                Timestamp = DateTime.UtcNow
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Gate);
        }

        [Fact]
        public void Should_Fail_When_Gate_Exceeds_Max_Length()
        {
            var dto = new SensorEventCreateDto
            {
                Gate = new string('A', 51),
                NumberOfPeople = 10,
                Type = "Enter",
                Timestamp = DateTime.UtcNow
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Gate);
        }

        [Fact]
        public void Should_Fail_When_NumberOfPeople_Is_Zero_Or_Less()
        {
            var dto = new SensorEventCreateDto
            {
                Gate = "Main Gate",
                NumberOfPeople = 0,
                Type = "Enter",
                Timestamp = DateTime.UtcNow
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.NumberOfPeople);
        }

        [Theory]
        [InlineData("")]
        [InlineData("InvalidType")]
        public void Should_Fail_When_Type_Is_Invalid(string type)
        {
            var dto = new SensorEventCreateDto
            {
                Gate = "Main Gate",
                NumberOfPeople = 10,
                Type = type,
                Timestamp = DateTime.UtcNow
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Type);
        }

        [Fact]
        public void Should_Pass_For_Type_Enter_Or_Leave_CaseInsensitive()
        {
            var dto1 = new SensorEventCreateDto
            {
                Gate = "Main Gate",
                NumberOfPeople = 10,
                Type = "enter",
                Timestamp = DateTime.UtcNow
            };
            var dto2 = new SensorEventCreateDto
            {
                Gate = "Main Gate",
                NumberOfPeople = 10,
                Type = "LEAVE",
                Timestamp = DateTime.UtcNow
            };

            _validator.TestValidate(dto1).ShouldNotHaveValidationErrorFor(x => x.Type);
            _validator.TestValidate(dto2).ShouldNotHaveValidationErrorFor(x => x.Type);
        }

        [Fact]
        public void Should_Fail_When_Timestamp_Is_Default()
        {
            var dto = new SensorEventCreateDto
            {
                Gate = "Main Gate",
                NumberOfPeople = 10,
                Type = "Enter",
                Timestamp = default
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Timestamp);
        }
    }
}