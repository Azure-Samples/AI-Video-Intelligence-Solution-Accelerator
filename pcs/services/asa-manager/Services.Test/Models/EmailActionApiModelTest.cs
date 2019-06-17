// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test.Models
{
    public class EmailActionApiModelTest
    {
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void EmptyActionsAreEqual()
        {
            // Arrange
            var action = new EmailActionApiModel();
            var action2 = new EmailActionApiModel();

            // Assert
            Assert.Equal(action, action2);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ActionsAreEqual_WithNoParameters()
        {
            // Arrange: action without parameters
            var action = new EmailActionApiModel()
            {
                Type = ActionType.Email
            };

            var action2 = Clone(action);

            // Assert
            Assert.Equal(action, action2);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ActionsAreEqual_WithParameters()
        {
            // Arrange: action with parameters
            var action = new EmailActionApiModel()
            {
                Type = ActionType.Email,
                Parameters = this.CreateSampleParameters()
            };
            var action2 = Clone(action);

            // Assert
            Assert.Equal(action, action2);
        }
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ActionsWithDifferentDataAreDifferent()
        {
            // Arrange
            var action = new EmailActionApiModel()
            {
                Type = ActionType.Email,
                Parameters = this.CreateSampleParameters()
            };

            var action2 = Clone(action);
            var action3 = Clone(action);

            action2.Parameters.Add("key1", "x");
            action3.Parameters["Notes"] += "sample string";

            // Assert
            Assert.NotEqual(action, action2);
            Assert.NotEqual(action, action3);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ActionsWithDifferentKeysAreDifferent()
        {
            // Arrange: different number of key-value pairs in Parameters.
            var action = new EmailActionApiModel()
            {
                Parameters = this.CreateSampleParameters()
            };

            var action2 = Clone(action);
            action2.Parameters.Add("Key1", "Value1");

            // Assert
            Assert.NotEqual(action, action2);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ActionsWithDifferentKayValuesAreDifferent()
        {
            // Arrange: different template
            var action = new EmailActionApiModel()
            {
                Parameters = this.CreateSampleParameters()
            };

            var action2 = Clone(action);
            action2.Parameters["Notes"] = "Changing note";

            // Assert
            Assert.NotEqual(action, action2);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ActionsWithDifferentRecipientsAreDifferent()
        {
            //Arrange: Differet list of email
            var action = new EmailActionApiModel
            {
                Parameters = this.CreateSampleParameters()
            };
            var action2 = Clone(action);
            action2.Parameters["Recipients"] = new List<string> { "sampleEmail1@gmail.com", "sampleEmail2@gmail.com", "samleEmail3@gmail.com" };

            // Assert
            Assert.NotEqual(action, action2);

            // Arrange: Different list of email, same length
            action2.Parameters["Recipients"] = new List<string>() { "anotherEmail1@gmail.com", "anotherEmail2@gmail.com" };

            // Assert
            Assert.NotEqual(action, action2);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ActionsWithSameRecipientsAreSame()
        {
            // Arrange: Same list of email different order.
            var action = new EmailActionApiModel()
            {
                Parameters = this.CreateSampleParameters()
            };
            var action2 = Clone(action);
            action2.Parameters["Recipients"] = new List<string>() { "sampleEmail2@gmail.com", "sampleEmail1@gmail.com" };

            // Assert
            Assert.Equal(action, action2);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ActionComparisonIsCaseInsensitive()
        {
            // Arrange: Same list of email different order.
            var actionDict = new Dictionary<string, object>()
            {
                { "Type", "Email" },
                { "Parameters", this.CreateSampleParameters() }
            };

            var actionDict2 = new Dictionary<string, object>()
            {
                { "Type", "Email" },
                { "Parameters", new Dictionary<string, object>()
                {
                    { "noTeS", "Sample Note" },
                    { "REcipienTs", new List<string>() {"sampleEmail2@gmail.com", "sampleEmail1@gmail.com"} }
                } }
            };

            var jsonAction = JsonConvert.SerializeObject(actionDict);
            var jsonAction2 = JsonConvert.SerializeObject(actionDict2);
            var action = JsonConvert.DeserializeObject<EmailActionApiModel>(jsonAction);
            var action2 = JsonConvert.DeserializeObject<EmailActionApiModel>(jsonAction2);
            Assert.Equal(action, action2);
        }

        private static T Clone<T>(T o)
        {
            var a = JsonConvert.SerializeObject(o);
            return JsonConvert.DeserializeObject<T>(
                JsonConvert.SerializeObject(o));
        }

        private Dictionary<string, object> CreateSampleParameters()
        {
            return new Dictionary<string, object>()
            {
                { "Notes", "Sample Note" },
                { "Recipients", new List<string> { "sampleEmail1@gmail.com", "sampleEmail2@gmail.com" } }
            };
        }
    }
}
