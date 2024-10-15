using System.IO;
using Seeq.Link.SDK.Services;
using Seeq.Link.SDK.Utilities;
using Seeq.Utilities;

namespace Seeq.Link.Debugging.Agent {

    internal static class AgentOtpHelper {
        private const string AGENT_ONE_TIME_PASSWORD_PLACEHOLDER = "<your_agent_one_time_password>";
        private static string OtpFilePath = Path.Combine("data", "keys", "agent.otp");

        public static void SetupAgentOtp(string seeqDataFolder, string agentName) {
            if (isAgentOneTimePasswordSet()) {
                var agentHelper = new AgentHelper(agentName);
                var secretsPath = Path.Combine(seeqDataFolder, SeeqNames.Agents.AgentKeysFolderName, "agent.keys");
                var secretsManager = new FileBasedSecretsManager(secretsPath);

                // set the agent's pre-provisioned one-time password
                var agentOneTimePassword = readAgentOneTimePassword();
                var preProvisionedOneTimePasswordSecretName =
                    $"{agentHelper.ProvisionedAgentUsername}|PRE_PROVISIONED_ONE_TIME_PASSWORD";
                secretsManager.PutSecret(preProvisionedOneTimePasswordSecretName, agentOneTimePassword);

                // clear the OTP
                resetAgentOneTimePasswordFile();
            }
        }

        private static bool isAgentOneTimePasswordSet() {
            var fileContent = readAgentOneTimePassword();
            return fileContent != AGENT_ONE_TIME_PASSWORD_PLACEHOLDER;
        }

        private static string readAgentOneTimePassword() {
            if (!File.Exists(OtpFilePath)) {
                return null;
            }

            return File.ReadAllText(OtpFilePath).Trim();
        }

        private static void resetAgentOneTimePasswordFile() {
            File.WriteAllText(OtpFilePath, AGENT_ONE_TIME_PASSWORD_PLACEHOLDER);
        }
    }
}