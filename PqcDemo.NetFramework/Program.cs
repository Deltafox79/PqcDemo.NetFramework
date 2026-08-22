using System;
using System.Linq;
using System.Text;
using System.Globalization;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Kems;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Pqc.Crypto.Falcon;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Spectre.Console;

namespace PqcDemo.NetFramework
{
    internal class Program
    {
        static bool isIta;

        static void Main(string[] args)
        {
            string osLang = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            
            var langChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select language / Seleziona lingua:")
                    .AddChoices(new string[] { "1. Italiano", "2. English" })
                    .EnableSearch()
                    .SearchPlaceholderText("(Type to search / Digita per cercare)")
            );

            isIta = langChoice.Contains("Italiano");

            string searchPlaceholder = isIta ? "(Digita per cercare)" : "(Type to search)";

            string optKem = isIta ? "1. ML-KEM (Incapsulamento Chiavi)" : "1. ML-KEM (Key Encapsulation)";
            string optDsa = isIta ? "2. ML-DSA (Firma basata su Reticoli)" : "2. ML-DSA (Lattice Signature)";
            string optSlh = isIta ? "3. SLH-DSA (Firma basata su Hash)" : "3. SLH-DSA (Hash-based Signature)";
            string optFnDsa = isIta ? "4. FN-DSA / FALCON (Firma compatta su Reticoli)" : "4. FN-DSA / FALCON (Compact Lattice Signature)";
            string optExit = isIta ? "0. Esci dal programma" : "0. Exit program";
            string optBack = isIta ? "0. <- Indietro" : "0. <- Go Back";

            while (true)
            {
                AnsiConsole.Clear();
                
                AnsiConsole.MarkupLine("[bold cyan]=================================================[/]");
                AnsiConsole.MarkupLine("[bold cyan]             PQC Demo Suite v1.1                 [/]");
                AnsiConsole.MarkupLine("[bold cyan]=================================================[/]\n");

                string titleAlgo = isIta ? "Scegli l'[green]algoritmo[/] da eseguire (usa frecce o numeri):" : "Choose the [green]algorithm[/] to run (use arrows or numbers):";
                var algorithm = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title(titleAlgo)
                        .AddChoices(new string[] { optKem, optDsa, optSlh, optFnDsa, optExit })
                        .EnableSearch()
                        .SearchPlaceholderText(searchPlaceholder));

                if (algorithm == optExit) break;

                string[] securityChoices = new string[0];

                if (algorithm == optKem)
                {
                    securityChoices = new string[] {
                        isIta ? "1. ML-KEM-512 (Livello 1 - Equivalente AES-128)" : "1. ML-KEM-512 (Level 1 - AES-128 eq.)",
                        isIta ? "2. ML-KEM-768 (Livello 3 - Equivalente AES-192)" : "2. ML-KEM-768 (Level 3 - AES-192 eq.)",
                        isIta ? "3. ML-KEM-1024 (Livello 5 - Equivalente AES-256)" : "3. ML-KEM-1024 (Level 5 - AES-256 eq.)",
                        optBack
                    };
                }
                else if (algorithm == optDsa)
                {
                    securityChoices = new string[] {
                        isIta ? "1. ML-DSA-44 (Livello 2 - Equivalente AES-128)" : "1. ML-DSA-44 (Level 2 - AES-128 eq.)",
                        isIta ? "2. ML-DSA-65 (Livello 3 - Equivalente AES-192)" : "2. ML-DSA-65 (Level 3 - AES-192 eq.)",
                        isIta ? "3. ML-DSA-87 (Livello 5 - Equivalente AES-256)" : "3. ML-DSA-87 (Level 5 - AES-256 eq.)",
                        optBack
                    };
                }
                else if (algorithm == optFnDsa)
                {
                    securityChoices = new string[] {
                        isIta ? "1. FN-DSA-512 (Livello 1 - Equivalente AES-128)" : "1. FN-DSA-512 (Level 1 - AES-128 eq.)",
                        isIta ? "2. FN-DSA-1024 (Livello 5 - Equivalente AES-256)" : "2. FN-DSA-1024 (Level 5 - AES-256 eq.)",
                        optBack
                    };
                }
                else if (algorithm == optSlh)
                {
                    securityChoices = new string[] {
                        isIta ? "1. SLH-DSA-128 (Livello 1 - Equivalente AES-128)" : "1. SLH-DSA-128 (Level 1 - AES-128 eq.)",
                        isIta ? "2. SLH-DSA-192 (Livello 3 - Equivalente AES-192)" : "2. SLH-DSA-192 (Level 3 - AES-192 eq.)",
                        isIta ? "3. SLH-DSA-256 (Livello 5 - Equivalente AES-256)" : "3. SLH-DSA-256 (Level 5 - AES-256 eq.)",
                        optBack
                    };
                }

                string titleSec = isIta ? "Seleziona il [yellow]Parametro di Sicurezza (NIST)[/]:" : "Select [yellow]NIST Security Parameter[/]:";
                var securityLevel = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title(titleSec)
                        .AddChoices(securityChoices)
                        .EnableSearch()
                        .SearchPlaceholderText(searchPlaceholder)
                );

                if (securityLevel == optBack) continue;

                if (algorithm == optKem)
                {
                    RunMlKem(securityLevel);
                }
                else if (algorithm == optDsa)
                {
                    RunMldsa(securityLevel);
                }
                else if (algorithm == optFnDsa)
                {
                    var paddingChoice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title(isIta ? "Seleziona il [yellow]Formato Firma FN-DSA[/]:" : "Select [yellow]FN-DSA Signature Format[/]:")
                            .AddChoices(new string[]
                            {
                                isIta ? "1. Padded (Dimensione fissa: 666 B / 1280 B)" : "1. Padded (Fixed size: 666 B / 1280 B)",
                                isIta ? "2. Unpadded (Dimensione variabile compressa)" : "2. Unpadded (Compressed variable size)",
                                optBack
                            })
                            .EnableSearch()
                            .SearchPlaceholderText(searchPlaceholder));

                    if (paddingChoice == optBack) continue;

                    RunFnDsa(securityLevel, paddingChoice.StartsWith("1."));
                }
                else if (algorithm == optSlh)
                {
                    var hashType = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title(isIta ? "Seleziona la [yellow]Funzione Hash[/] per SLH-DSA:" : "Select SLH-DSA [yellow]Hash Function[/]:")
                            .AddChoices(new string[] { "1. SHA-2", "2. SHAKE", optBack })
                            .EnableSearch()
                            .SearchPlaceholderText(searchPlaceholder));

                    if (hashType == optBack) continue;

                    var optimization = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title(isIta ? "Seleziona la [yellow]Strategia di Ottimizzazione[/]:" : "Select SLH-DSA [yellow]Optimization Strategy[/]:")
                            .AddChoices(new string[]
                            {
                                isIta ? "1. Small (s) - Firma più piccola, esecuzione più lenta" : "1. Small (s) - Smaller signature, slower execution",
                                isIta ? "2. Fast (f)  - Esecuzione più veloce, firma più grande" : "2. Fast (f)  - Faster execution, larger signature",
                                optBack
                            })
                            .EnableSearch()
                            .SearchPlaceholderText(searchPlaceholder));

                    if (optimization == optBack) continue;

                    RunSlhdsa(securityLevel, hashType.Contains("SHA-2"), optimization.Contains("Small"));
                }

                AnsiConsole.MarkupLine(isIta ? "\n[bold gray]Premi un tasto per tornare al menu principale...[/]" : "\n[bold gray]Press any key to return to the main menu...[/]");
                Console.ReadKey(true);
            }
        }

        static void RunFnDsa(string level, bool isPadded)
        {
            bool is1024 = level.Contains("1024");
            FalconParameters param = is1024 ? FalconParameters.falcon_1024 : FalconParameters.falcon_512;
            int targetLength = is1024 ? 1280 : 666;

            Console.WriteLine("***************** FN-DSA / FALCON (" + param.Name.ToUpper().Replace("_", "-") + (isPadded ? " PADDED" : " UNPADDED") + ") *******************");

            var raw = isIta ? "Ciao, sono Alice e questa è una firma PQC Compatta!" : "Hello, I'm Alice and this is a Compact PQC Signature!";
            var data = Hex.Encode(Encoding.ASCII.GetBytes(raw));

            var random = new SecureRandom();
            var keyGenParameters = new FalconKeyGenerationParameters(random, param);
            var keyPairGenerator = new FalconKeyPairGenerator();
            keyPairGenerator.Init(keyGenParameters);

            var keyPair = keyPairGenerator.GenerateKeyPair();
            var publicKey = (FalconPublicKeyParameters)keyPair.Public;
            var privateKey = (FalconPrivateKeyParameters)keyPair.Private;
            var pubEncoded = publicKey.GetEncoded();
            var privateEncoded = privateKey.GetEncoded();

            PrintPanel(isIta ? "Chiavi" : "Keys", new string[] { 
                "[green]PUB[/] (" + pubEncoded.Length + " bytes): " + pubEncoded.PrettyPrint(), 
                "[red]PRV[/] (" + privateEncoded.Length + " bytes): " + privateEncoded.PrettyPrint() 
            });

            var alice = new FalconSigner();
            alice.Init(true, privateKey);
            var rawSignature = alice.GenerateSignature(data);

            byte[] displaySignature;
            if (isPadded)
            {
                displaySignature = new byte[targetLength];
                Array.Copy(rawSignature, displaySignature, rawSignature.Length);
            }
            else
            {
                displaySignature = rawSignature;
            }

            PrintPanel("Data", new string[] { 
                "[blue]RAW[/]: " + raw, 
                "[blue]HEX[/]: " + data.PrettyPrint(), 
                "[yellow]SIG[/] (" + displaySignature.Length + " bytes): " + displaySignature.PrettyPrint() 
            });

            byte[] signatureToVerify = rawSignature; 

            var bob = new FalconSigner();
            bob.Init(false, publicKey);
            var verified = bob.VerifySignature(data, signatureToVerify);

            PrintPanel(isIta ? "Verifica" : "Verification", new string[] { 
                (verified ? "[green]OK[/]" : "[red]ERRORE[/]") + (isIta ? " Verificata!" : " Verified!") 
            });
        }

        static void RunSlhdsa(string level, bool useSha2, bool isSmall)
        {
            SlhDsaParameters param = SlhDsaParameters.slh_dsa_sha2_128s;

            if (level.Contains("128")) 
            {
                if (useSha2) param = isSmall ? SlhDsaParameters.slh_dsa_sha2_128s : SlhDsaParameters.slh_dsa_sha2_128f;
                else param = isSmall ? SlhDsaParameters.slh_dsa_shake_128s : SlhDsaParameters.slh_dsa_shake_128f;
            }
            else if (level.Contains("192")) 
            {
                if (useSha2) param = isSmall ? SlhDsaParameters.slh_dsa_sha2_192s : SlhDsaParameters.slh_dsa_sha2_192f;
                else param = isSmall ? SlhDsaParameters.slh_dsa_shake_192s : SlhDsaParameters.slh_dsa_shake_192f;
            }
            else if (level.Contains("256")) 
            {
                if (useSha2) param = isSmall ? SlhDsaParameters.slh_dsa_sha2_256s : SlhDsaParameters.slh_dsa_sha2_256f;
                else param = isSmall ? SlhDsaParameters.slh_dsa_shake_256s : SlhDsaParameters.slh_dsa_shake_256f;
            }

            Console.WriteLine("***************** SLH-DSA (" + param.Name.ToUpper().Replace("_", "-") + ") *******************");

            var raw = isIta ? "Ciao, sono Alice e questa è una firma PQC stateless basata su Hash!" : "Hello, I'm Alice and this is a Stateless Hash-Based Signature!";
            var data = Hex.Encode(Encoding.ASCII.GetBytes(raw));

            var random = new SecureRandom();
            var keyGenParameters = new SlhDsaKeyGenerationParameters(random, param);
            var slhdsaKeyPairGenerator = new SlhDsaKeyPairGenerator();
            slhdsaKeyPairGenerator.Init(keyGenParameters);

            var keyPair = slhdsaKeyPairGenerator.GenerateKeyPair();
            var publicKey = (SlhDsaPublicKeyParameters)keyPair.Public;
            var privateKey = (SlhDsaPrivateKeyParameters)keyPair.Private;
            var pubEncoded = publicKey.GetEncoded();
            var privateEncoded = privateKey.GetEncoded();

            PrintPanel(isIta ? "Chiavi" : "Keys", new string[] {
                "[green]PUB[/] (" + pubEncoded.Length + " bytes): " + pubEncoded.PrettyPrint(),
                "[red]PRV[/] (" + privateEncoded.Length + " bytes): " + privateEncoded.PrettyPrint()
            });

            var alice = new SlhDsaSigner(param, true);
            alice.Init(true, privateKey);
            alice.BlockUpdate(data, 0, data.Length);
            var signature = alice.GenerateSignature();

            PrintPanel("Data", new string[] {
                "[blue]RAW[/]: " + raw,
                "[blue]HEX[/]: " + data.PrettyPrint(),
                "[yellow]SIG[/] (" + signature.Length + " bytes): " + signature.PrettyPrint()
            });

            var bob = new SlhDsaSigner(param, true);
            bob.Init(false, publicKey);
            bob.BlockUpdate(data, 0, data.Length);
            var verified = bob.VerifySignature(signature);

            PrintPanel(isIta ? "Verifica" : "Verification", new string[] { 
                (verified ? "[green]OK[/]" : "[red]ERRORE[/]") + (isIta ? " Verificata!" : " Verified!") 
            });
        }

        static void RunMldsa(string level)
        {
            MLDsaParameters param = MLDsaParameters.ml_dsa_65;
            
            if (level.Contains("44")) param = MLDsaParameters.ml_dsa_44; 
            else if (level.Contains("65")) param = MLDsaParameters.ml_dsa_65;
            else if (level.Contains("87")) param = MLDsaParameters.ml_dsa_87;

            Console.WriteLine("***************** ML-DSA (" + param.Name.ToUpper().Replace("_", "-") + ") *******************");

            var raw = isIta ? "Ciao, sono Alice e puoi verificare questa firma!" : "Hello, I'm Alice and you can verify that!";
            var data = Hex.Encode(Encoding.ASCII.GetBytes(raw));

            var random = new SecureRandom();
            var keyGenParameters = new MLDsaKeyGenerationParameters(random, param);
            var mldsaKeyPairGenerator = new MLDsaKeyPairGenerator();
            mldsaKeyPairGenerator.Init(keyGenParameters);

            var keyPair = mldsaKeyPairGenerator.GenerateKeyPair();
            var publicKey = (MLDsaPublicKeyParameters)keyPair.Public;
            var privateKey = (MLDsaPrivateKeyParameters)keyPair.Private;
            var pubEncoded = publicKey.GetEncoded();
            var privateEncoded = privateKey.GetEncoded();

            PrintPanel(isIta ? "Chiavi" : "Keys", new string[] { 
                "[green]PUB[/] (" + pubEncoded.Length + " bytes): " + pubEncoded.PrettyPrint(), 
                "[red]PRV[/] (" + privateEncoded.Length + " bytes): " + privateEncoded.PrettyPrint() 
            });

            var alice = new MLDsaSigner(param, true);
            alice.Init(true, privateKey);
            alice.BlockUpdate(data, 0, data.Length);
            var signature = alice.GenerateSignature();

            PrintPanel("Data", new string[] { 
                "[blue]RAW[/]: " + raw, 
                "[blue]HEX[/]: " + data.PrettyPrint(), 
                "[yellow]SIG[/] (" + signature.Length + " bytes): " + signature.PrettyPrint() 
            });

            var bob = new MLDsaSigner(param, true);
            bob.Init(false, publicKey);
            bob.BlockUpdate(data, 0, data.Length);
            var verified = bob.VerifySignature(signature);

            PrintPanel(isIta ? "Verifica" : "Verification", new string[] { 
                (verified ? "[green]OK[/]" : "[red]ERRORE[/]") + (isIta ? " Verificata!" : " Verified!") 
            });
        }

        static void RunMlKem(string level)
        {
            MLKemParameters param = MLKemParameters.ml_kem_768;
            
            if (level.Contains("512")) param = MLKemParameters.ml_kem_512;
            else if (level.Contains("768")) param = MLKemParameters.ml_kem_768;
            else if (level.Contains("1024")) param = MLKemParameters.ml_kem_1024;

            Console.WriteLine("***************** ML-KEM (" + param.Name.ToUpper().Replace("_", "-") + ") *******************");

            var random = new SecureRandom();
            var keyGenParameters = new MLKemKeyGenerationParameters(random, param);
            var kyberKeyPairGenerator = new MLKemKeyPairGenerator();
            kyberKeyPairGenerator.Init(keyGenParameters);

            var aliceKeyPair = kyberKeyPairGenerator.GenerateKeyPair();
            var alicePublic = (MLKemPublicKeyParameters)aliceKeyPair.Public;
            var alicePrivate = (MLKemPrivateKeyParameters)aliceKeyPair.Private;
            var pubEncoded = alicePublic.GetEncoded();
            var privateEncoded = alicePrivate.GetEncoded();

            PrintPanel(isIta ? "Chiavi di Alice" : "Alice's keys", new string[] { 
                "[green]PUB[/] (" + pubEncoded.Length + " bytes): " + pubEncoded.PrettyPrint(), 
                "[red]PRV[/] (" + privateEncoded.Length + " bytes): " + privateEncoded.PrettyPrint() 
            });

            var encapsulator = new MLKemEncapsulator(param);
            encapsulator.Init(MLKemPublicKeyParameters.FromEncoding(param, pubEncoded));
            var cipherText = new byte[encapsulator.EncapsulationLength];
            var bobSecret = new byte[encapsulator.SecretLength];
            encapsulator.Encapsulate(cipherText, 0, cipherText.Length, bobSecret, 0, bobSecret.Length);

            var decapsulator = new MLKemDecapsulator(param);
            decapsulator.Init(alicePrivate);
            var aliceSecret = new byte[decapsulator.SecretLength];
            decapsulator.Decapsulate(cipherText, 0, cipherText.Length, aliceSecret, 0, aliceSecret.Length);

            PrintPanel(isIta ? "Incapsulamento Chiave" : "Key encapsulation", new string[] { 
                "[yellow]BOB SEC[/] (" + bobSecret.Length + " bytes): " + bobSecret.PrettyPrint(), 
                "[blue]CIPHER[/]  (" + cipherText.Length + " bytes): " + cipherText.PrettyPrint() 
            });
            PrintPanel(isIta ? "Decapsulamento Chiave" : "Key decapsulation", new string[] { 
                "[yellow]ALI SEC[/] (" + aliceSecret.Length + " bytes): " + aliceSecret.PrettyPrint() 
            });

            var equal = bobSecret.SequenceEqual(aliceSecret);
            
            PrintPanel(isIta ? "Verifica" : "Verification", new string[] { 
                (equal ? "[green]OK[/]" : "[red]ERRORE[/]") + (isIta ? " Segreti identici!" : " Secrets equal!") 
            });
        }

        static void PrintPanel(string header, string[] data)
        {
            var content = string.Join(Environment.NewLine, data);
            var panel = new Panel(content)
            {
                Header = new PanelHeader(header)
            };
            AnsiConsole.Write(panel);
        }
    }

    public static class FormatExtensions
    {
        public static string PrettyPrint(this byte[] bytes)
        {
            var base64 = Convert.ToBase64String(bytes);
            return base64.Length > 50 ? base64.Substring(0, 25) + "..." + base64.Substring(base64.Length - 25) : base64;
        }
    }
}