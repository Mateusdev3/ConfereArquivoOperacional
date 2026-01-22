using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Compression;

namespace ConfereOperacional
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            WriteTitle("CONFEREOPERACIONAL");
            Console.Write("Insira a data da pasta (ex: 20260119): ");
            string data = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(data))
            {
                WriteError("Data inválida.");
                PauseAndExit();
                return;
            }

            string caminhoPasta = Path.Combine(baseDirectory, data);
            if (!Directory.Exists(caminhoPasta))
            {
                WriteError("Pasta não existe: " + caminhoPasta);
                PauseAndExit();
                return;
            }

            var folders = Directory.GetDirectories(caminhoPasta).Distinct().ToArray();

       
            string[] arquivosObrigatoriosPainel =
            {
                "buslines.upex",
                "confgps.upex",
                "messages.upex",
                "motorista.upex",
                "tripschedule.upex",
                "version.txt"
            };

            string pathTxt = Path.Combine(baseDirectory, $"{data}_logs.txt");

            // Cabeçalho do log
            File.WriteAllLines(pathTxt, new[]
            {
                "========================================",
                "ConfereOperacional - Relatório",
                $"Gerado em: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                $"Pasta analisada: {data}",
                "========================================",
                ""
            });

            if (folders.Length != 36)
                WriteWarn($"Número de operadoras diferente de 36 ({folders.Length}).");

            int totalOk = 0;
            int totalErro = 0;
            int totalPainelErro = 0;

            WriteInfo($"Operadoras encontradas: {folders.Length}");
            Console.WriteLine();

            foreach (string folder in folders)
            {
                string operadora = Path.GetFileName(folder);

                string[] tacs = Directory.GetFiles(folder, "*.tac", SearchOption.AllDirectories);

                bool okTacCount = tacs.Length == 3;

                string painelTac = tacs.FirstOrDefault(f =>
                    Path.GetFileName(f).Equals("operacional-painel.tac", StringComparison.OrdinalIgnoreCase));

                bool painelOk = false;
                List<string> faltandoPainel = new();
                string painelStatusMsg;

                if (painelTac is null)
                {
                    painelStatusMsg = "NÃO ENCONTRADO";
                }
                else
                {
                    var (ok, faltando, erroZip) = ValidarZipPainel(painelTac, arquivosObrigatoriosPainel);

                    if (erroZip)
                    {
                        painelStatusMsg = "ZIP INVÁLIDO / CORROMPIDO";
                    }
                    else if (!ok)
                    {
                        painelStatusMsg = $"FALTANDO ({faltando.Count})";
                        faltandoPainel = faltando;
                    }
                    else
                    {
                        painelStatusMsg = "OK";
                        painelOk = true;
                    }
                }

                bool operadoraOk = okTacCount && painelOk;

                WriteOperadoraHeader(operadora);

                if (okTacCount) WriteOk($"TACs: {tacs.Length}/3");
                else WriteError($"TACs: {tacs.Length}/3 (quantidade incorreta)");

                if (painelOk) WriteOk($"Painel: {painelStatusMsg}");
                else
                {
                    WriteError($"Painel: {painelStatusMsg}");

                    if (faltandoPainel.Count > 0)
                    {
                        WriteWarn("Arquivos ausentes dentro do operacional-painel.tac:");
                        foreach (var f in faltandoPainel)
                            Console.WriteLine($"   - {f}");
                        totalPainelErro++;
                    }
                }

                Console.WriteLine();
                var linhas = new List<string>
                {
                    $"[OPERADORA] {operadora}",
                    $"  - TACs: {tacs.Length}/3 {(okTacCount ? "OK" : "ERRO")}",
                    $"  - operacional-painel.tac: {painelStatusMsg}",
                    ""
                };

                linhas.Add("  [TACs ENCONTRADOS]");
                foreach (string t in tacs.Select(Path.GetFileName).OrderBy(x => x))
                    linhas.Add($"   - {t}");

                linhas.Add("");  

                if (painelTac is null)
                {
                    linhas.Add("  [PAINEL]");
                    linhas.Add("   - ERRO: operacional-painel.tac não encontrado");
                }
                else if (!painelOk)
                {
                    linhas.Add("  [PAINEL]");
                    linhas.Add($"   - Arquivo: {Path.GetFileName(painelTac)}");
                    linhas.Add($"   - Status: {painelStatusMsg}");

                    if (faltandoPainel.Count > 0)
                    {
                        linhas.Add("   - Faltando:");
                        foreach (var f in faltandoPainel)
                            linhas.Add($"      * {f}");
                    }
                }
                else
                {
                    linhas.Add("  [PAINEL]");
                    linhas.Add($"   - Arquivo: {Path.GetFileName(painelTac)}");
                    linhas.Add("   - Status: OK (todos os arquivos presentes)");
                }

                linhas.Add("----------------------------------------");
                linhas.Add("");
                File.AppendAllLines(pathTxt, linhas);

                if (operadoraOk) totalOk++;
                else totalErro++;
            }
            WriteTitle("RESUMO");
            WriteInfo($"OK: {totalOk}");
            if (totalErro > 0) WriteError($"ERROS: {totalErro}");
            else WriteOk("ERROS: 0");

            if (totalPainelErro > 0) WriteWarn($"Operadoras com falta de arquivos no painel: {totalPainelErro}");

            Console.WriteLine();
            WriteInfo($"Log gerado em: {pathTxt}");
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }
        static (bool ok, List<string> faltando, bool erroZip) ValidarZipPainel(string tacPath, string[] obrigatorios)
        {
            try
            {
                using ZipArchive zip = ZipFile.OpenRead(tacPath);

                var arquivosNoZip = zip.Entries
                    .Select(e => Path.GetFileName(e.FullName))
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(n => n.ToLowerInvariant())
                    .ToHashSet();

                var faltando = obrigatorios
                    .Where(o => !arquivosNoZip.Contains(o.ToLowerInvariant()))
                    .ToList();

                return (faltando.Count == 0, faltando, false);
            }
            catch
            {
                return (false, new List<string>(), true);
            }
        }
        static void WriteTitle(string text)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("========================================");
            Console.WriteLine(text);
            Console.WriteLine("========================================");
            Console.ResetColor();
        }

        static void WriteOperadoraHeader(string operadora)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"▶ Operadora {operadora}");
            Console.ResetColor();
        }

        static void WriteOk(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ " + msg);
            Console.ResetColor();
        }

        static void WriteWarn(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ! " + msg);
            Console.ResetColor();
        }

        static void WriteError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ✗ " + msg);
            Console.ResetColor();
        }

        static void WriteInfo(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        static void PauseAndExit()
        {
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }
    }
}
