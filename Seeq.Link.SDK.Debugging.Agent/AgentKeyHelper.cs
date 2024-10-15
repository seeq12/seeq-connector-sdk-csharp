using System.IO;

namespace Seeq.Link.Debugging.Agent {

    internal static class AgentKeyHelper {
        private const string AGENT_ONE_TIME_PASSWORD_PLACEHOLDER = "<your_agent_one_time_password>";
        private static string OtpFilePath = Path.Combine("data", "keys", "agent.otp");

        public static bool IsAgentOneTimePasswordSet() {
            var fileContent = ReadAgentOneTimePassword();
            return fileContent != AGENT_ONE_TIME_PASSWORD_PLACEHOLDER;
        }

        public static string ReadAgentOneTimePassword() {
            if (!File.Exists(OtpFilePath)) {
                return null;
            }

            return File.ReadAllText(OtpFilePath).Trim();
        }

        public static void ResetAgentOneTimePasswordFile() {
            File.WriteAllText(OtpFilePath, AGENT_ONE_TIME_PASSWORD_PLACEHOLDER);
        }
    }
}