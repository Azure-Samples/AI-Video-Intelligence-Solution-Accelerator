// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Newtonsoft.Json.Linq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class ActionTest
    {
        private const string PARAM_NOTES = "Chiller pressure is at 250 which is high";
        private const string PARAM_SUBJECT = "Alert Notification";
        private const string PARAM_RECIPIENTS = "sampleEmail@gmail.com";
        private const string PARAM_SUBJECT_KEY = "Subject";
        private const string PARAM_NOTES_KEY = "Notes";
        private const string PARAM_RECIPIENTS_KEY = "Recipients";

        private readonly JArray emailArray;

        public ActionTest()
        {
            this.emailArray = new JArray { PARAM_RECIPIENTS };
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ReturnActionModel_When_ValidActionType()
        {
            // Arrange
            var parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { PARAM_SUBJECT_KEY, PARAM_SUBJECT },
                { PARAM_NOTES_KEY, PARAM_NOTES },
                { PARAM_RECIPIENTS_KEY, this.emailArray }
            };

            // Act 
            var result = new EmailAction(parameters);

            // Assert 
            Assert.Equal(ActionType.Email, result.Type);
            Assert.Equal(PARAM_NOTES, result.Parameters[PARAM_NOTES_KEY]);
            Assert.Equal(this.emailArray, result.Parameters[PARAM_RECIPIENTS_KEY]);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_When_ActionTypeIsEmailAndInvalidEmail()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                { PARAM_SUBJECT_KEY, PARAM_SUBJECT },
                { PARAM_NOTES_KEY, PARAM_NOTES },
                { PARAM_RECIPIENTS_KEY, new JArray() { "sampleEmailgmail.com"} }
            };

            // Act and Assert
            Assert.Throws<InvalidInputException>(() => new EmailAction(parameters));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_Throw_InvalidInputException_WhenActionTypeIsEmailAndNoRecipients()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                { PARAM_SUBJECT_KEY, PARAM_SUBJECT },
                { PARAM_NOTES_KEY, PARAM_NOTES }
            };

            // Act and Assert
            Assert.Throws<InvalidInputException>(() => new EmailAction(parameters));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_When_ActionTypeIsEmailAndEmailIsString()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                { PARAM_SUBJECT_KEY, PARAM_SUBJECT },
                { PARAM_NOTES_KEY, PARAM_NOTES },
                { PARAM_RECIPIENTS_KEY, PARAM_RECIPIENTS }
            };

            // Act and Assert
            Assert.Throws<InvalidInputException>(() => new EmailAction(parameters));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ReturnActionModel_When_ValidActionTypeParametersIsCaseInsensitive()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                { "subject", PARAM_SUBJECT },
                { "nOtEs", PARAM_NOTES },
                { "rEcipiEnts", this.emailArray }
            };

            // Act 
            var result = new EmailAction(parameters);

            // Assert 
            Assert.Equal(ActionType.Email, result.Type);
            Assert.Equal(PARAM_NOTES, result.Parameters[PARAM_NOTES_KEY]);
            Assert.Equal(this.emailArray, result.Parameters[PARAM_RECIPIENTS_KEY]);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_CreateAction_When_OptionalNotesAreMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                { PARAM_SUBJECT_KEY, PARAM_SUBJECT },
                { PARAM_RECIPIENTS_KEY, this.emailArray }
            };

            // Act 
            var result = new EmailAction(parameters);

            // Assert 
            Assert.Equal(ActionType.Email, result.Type);
            Assert.Equal(string.Empty, result.Parameters[PARAM_NOTES_KEY]);
            Assert.Equal(this.emailArray, result.Parameters[PARAM_RECIPIENTS_KEY]);
        }
    }
}
