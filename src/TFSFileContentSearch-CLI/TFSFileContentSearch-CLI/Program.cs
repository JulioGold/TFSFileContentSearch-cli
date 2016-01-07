using System;
using System.Collections.Generic;
using NDesk.Options;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.IO;
using System.Text;
using System.Linq;

namespace TFSFileContentSearch_CLI
{
    // Referências
    // http://www.ndesk.org/doc/ndesk-options/NDesk.Options/OptionSet.html
    // http://stackoverflow.com/a/6925420
    // http://pascallaurin42.blogspot.com.br/2012/05/tfs-queries-searching-in-all-files-of.html
    // http://blogs.msdn.com/b/taylaf/archive/2010/01/26/retrieve-the-list-of-team-project-collections-from-tfs-2010-client-apis.aspx

    class Program
    {
        private static string[] searchPatterns;
        private static string[] filePatterns;
        private static string server = string.Empty;
        private static string path = string.Empty;
        private static bool verbose = false;

        static void Main(string[] args)
        {
            bool showHelp = false;
            
            OptionSet optionSet = new OptionSet()
            {
                { "s|server=", "Server address with default collection.\nEx: http://tfsserver/DefaultCollection", v => server = v },
                { "p|path=", "Path to especific location on TFS where you want to find.\nEx: $/Project/Main/Source/Portal/Controllers/Api", v => path = v },
                { "sp|searchpatterns=", "Search patterns.", v => searchPatterns = v.Split(',') },
                { "fp|filepatterns=", "File paterns will filter the files where the search will do.", v => filePatterns = v.Split(',') },
                { "v|verbose-", "Show verbosity.", v => verbose = v != null },
                { "h|help",  "Show this message and exit.", v => showHelp = v != null }
            };

            List<string> extra;
            try
            {
                extra = optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine();
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `TFSFileContentSearch --help' for more information.");
                return;
            }

            // Processamento a partir daqui

            if (showHelp)
            {
                ShowHelp(optionSet);
                return;
            }
            
            if (searchPatterns != null &&
                filePatterns != null &&
                !string.IsNullOrEmpty(server) &&
                !string.IsNullOrEmpty(path))
            {

                if (verbose)
                {
                    Console.WriteLine("--------------------------------------------------------------------------------");
                    Console.WriteLine("Server: {0}", server);
                    Console.WriteLine("Path: {0}", path);
                    if (searchPatterns != null && searchPatterns.Length > 0)
                    {
                        foreach (string item in searchPatterns)
                        {
                            Console.WriteLine("Search pattern: {0}", item);
                        }
                    }
                    if (filePatterns != null && filePatterns.Length > 0)
                    {
                        foreach (string item in filePatterns)
                        {
                            Console.WriteLine("File pattern: {0}", item);
                        }
                    }
                    Console.WriteLine("--------------------------------------------------------------------------------");
                }

                try
                {
                    TfsTeamProjectCollection tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(server));

                    VersionControlServer versionControlServer = tfs.GetService<VersionControlServer>();
                
                    List<string> results = new List<string>();

                    foreach (string filePattern in filePatterns)
                    {
                        foreach (var item in versionControlServer.GetItems(string.Format("{0}{1}{2}", path.Trim(), path.Trim().EndsWith("/") ? "" : "/", filePattern), RecursionType.Full).Items)
                        {
                            results.AddRange(SearchInFileContent(item));
                        }
                    }

                    foreach (string item in results)
                    {
                        Console.WriteLine(item);
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }
            else /* Se não enviou os parâmetros corretamente, então exibe a ajuda. */
            {
                ShowHelp(optionSet);
                return;
            }
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine();
            Console.WriteLine("Usage: TFSFileContentSearch [OPTIONS]");
            Console.WriteLine("Find content inside files.");
            Console.WriteLine("{0}\n{1}", "Use it carefully.","Depending wath you specify on the path, the query may take a long time to be completed.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        private static List<string> SearchInFileContent(Item file)
        {
            // results contains the file path and all ocurrences
            List<string> results = new List<string>();
            // ocurrences contains the lines where
            List<string> occurrences = new List<string>();

            StreamReader streamReader = new StreamReader(file.DownloadFile(), Encoding.Default);

            string line = streamReader.ReadLine();
            int lineIndex = 0;

            while (!streamReader.EndOfStream)
            {
                if (searchPatterns.Any(item => line.IndexOf(item, StringComparison.OrdinalIgnoreCase) > 0))
                {
                    occurrences.Add(string.Format("Line {0}: {1}", lineIndex, line.Trim()));
                }

                line = streamReader.ReadLine();
                lineIndex++;
            }

            if (occurrences.Count > 0)
            {
                results.Add(file.ServerItem);
                results.AddRange(occurrences.Select(item => string.Format("\t{0}", item)).ToList());
            }

            return results;
        }
    }
}
