using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

// Generates human readable codes with a given number of digits.
public static class HumanFriendlyCodeGenerator {
    const int defaultCodeLength = 5;
    // A list of characters that are visually distinct from each other.
    // Omits certain characters such as all lower case characters and I and 1 (which can be confused)
    static char[] easilyRecognisableCharacters = {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '2', '3', '4', '5', '6', '7', '8', '9'};
	
    // Generates a human readable code with the default number of digits and valid characters.
    public static string Generate () => Generate(defaultCodeLength, easilyRecognisableCharacters);
    // Generates a human readable code with a given number of digits and the default valid characters.
    public static string Generate (int numCharacters) => Generate(numCharacters, easilyRecognisableCharacters);
    // Generates a human readable code with a given number of digits and an array of valid characters.
    public static string Generate(int numCharacters, char[] allowedCharacters) {
        var sb = new StringBuilder(numCharacters);
        for (int i = 0; i < numCharacters; i++) {
            int randomInt32 = RandomNumberGenerator.GetInt32(0, allowedCharacters.Length - 1);
            sb.Append(allowedCharacters[randomInt32]);
        }
        return sb.ToString();
    }

    // Generates a reproducable human readable code from a seed with the default number of digits and valid characters.
    public static string GenerateSeeded(int seed) => GenerateSeeded(seed, defaultCodeLength, easilyRecognisableCharacters);
    // Generates a reproducable human readable code from a seed with a given number of digits and the default valid characters.
    public static string GenerateSeeded(int seed, int numCharacters) => GenerateSeeded(seed, numCharacters, easilyRecognisableCharacters);
    // Generates a reproducable human readable code from a seed with a given number of digits and an array of valid characters.
    public static string GenerateSeeded(int seed, int numCharacters, char[] allowedCharacters) {
        var sb = new StringBuilder(numCharacters);
        Random seededRandom = new Random(seed);

        for (int i = 0; i < numCharacters; i++) {
            // Use seeded random to get a seed for RandomNumberGenerator
            int subSeed = seededRandom.Next();
            byte[] randomBytes = new byte[4];
            BitConverter.GetBytes(subSeed).CopyTo(randomBytes, 0);
            int randomInt32;
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(randomBytes);
                randomInt32 = BitConverter.ToInt32(randomBytes, 0) % allowedCharacters.Length;
                if (randomInt32 < 0) randomInt32 += allowedCharacters.Length;
            }

            sb.Append(allowedCharacters[randomInt32]);
        }

        return sb.ToString();
    }
    
    // Determines whether a code could have been made from this generator 
    public static bool CodeMatchesSignature(string code) => CodeMatchesSignature(code, defaultCodeLength, easilyRecognisableCharacters);
    // Determines whether a code could have been made from this generator 
    public static bool CodeMatchesSignature(string code, int numCharacters) => CodeMatchesSignature(code, numCharacters, easilyRecognisableCharacters);
    // Determines whether a code could have been made from this generator 
    public static bool CodeMatchesSignature(string code, int numCharacters, char[] allowedCharacters) => code != null && code.Length == numCharacters && code.All(allowedCharacters.Contains);
}