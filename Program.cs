using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarkdownGenerator
{
	class Program
	{
//		// 0 = dll src path, 1 = dest root
//		static void Main(string[] args)
//		{
//			// put dll & xml on same diretory.
//			var target = "";//UniRx.dll"; // :)
//			const string dest = "Docs";
//			if (args.Length == 1)
//			{
//				target = args[0];
//			}
//			else if (args.Length == 2)
//			{
//				target = args[0];
//				dest = args[1];
//			}
//
//			if(string.IsNullOrEmpty(target)){
//				Console.WriteLine("no args");
//				return;
//			}
//
//			try{
//				generate(target, dest);
//			}catch(Exception ex){
//				Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
//			}
//		}
		
		const string homeMdPrefix = "__Home__";
		
		static void Main(string[] args){
			
			var srcList = args.Where(i=> i.EndsWith(".dll") || i.EndsWith(".exe"));
			const string dest = "Docs";
			
			if( string.IsNullOrEmpty(srcList.FirstOrDefault()) ){
				Console.WriteLine("no args");
				return;
			}
			
			var logFi = new FileInfo("./_"+ System.AppDomain.CurrentDomain.FriendlyName
 +".err.log");
			//foreach( var i in srcList ) {
			Parallel.ForEach( srcList, i => {
			                 	try{
			                 		generate(i, dest);
			                 	}catch(Exception ex){
			                 		Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
			                 		logFi.WriteTextAsync($@"
{i}
	{ex.Message}
	{ex.StackTrace}
").Wait();
			                 	}
			                 });
			
			try{
				mergeHomes(dest);
			}catch(Exception ex){
				Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
				logFi.WriteTextAsync($@"
Err mergeHome
	{ex.Message}
	{ex.StackTrace}
").Wait();
			}
		}
		
		/// <summary>
		/// Combine multiple {homeMdPrefix}*.md and create Home.md
		/// </summary>
		/// <param name="dest"></param>
		static void mergeHomes(string dest){
			var homes = new DirectoryInfo(dest).EnumerateFiles($"{homeMdPrefix}*.md");
			var bufs = homes.AsParallel()
				.Select(
					i => File.ReadAllText(i.FullName)
					.Replace("# References", "# " + Regex.Replace(i.Name, $@"{homeMdPrefix}(.*)\.md", "$1") )
				).ToArray();
			
			new FileInfo(dest + @"/Home.md")
				.WriteTextAsync( string.Join(Environment.NewLine, bufs) ).Wait();
			
			foreach (var i in homes) {
				i.Delete();
			}
		}
		
		static void generate(string target, string dest){
			var types = MarkdownGenerator.Load(target);

			// Home Markdown Builder
			var homeBuilder = new MarkdownBuilder();
			homeBuilder.Header(1, "References");
			homeBuilder.AppendLine();

			foreach (var g in types.GroupBy(x => x.Namespace).OrderBy(x => x.Key))
			{
				if (!Directory.Exists(dest)) Directory.CreateDirectory(dest);

				homeBuilder.HeaderWithLink(2, g.Key, g.Key);
				homeBuilder.AppendLine();

				var sb = new StringBuilder();
				foreach (var item in g.OrderBy(x => x.Name))
				{
					homeBuilder.ListLink(MarkdownBuilder.MarkdownCodeQuote(item.BeautifyName), g.Key + "#" + item.BeautifyName.Replace("<", "").Replace(">", "").Replace(",", "").Replace(" ", "-").ToLower());

					sb.Append(item.ToString());
				}

				File.WriteAllText(Path.Combine(dest, g.Key + ".md"), sb.ToString());
				//File.WriteAllText(Path.Combine(dest, new FileInfo(target).Name + g.Key + ".md"), sb.ToString());
				homeBuilder.AppendLine();
			}

			// Gen Home
			//File.WriteAllText(Path.Combine(dest, "Home.md"), homeBuilder.ToString());
			File.WriteAllText(Path.Combine(dest, $"{homeMdPrefix}{new FileInfo(target).Name}.md"), homeBuilder.ToString());
		}
	}
	
	public static class Ex{
		public static async Task WriteTextAsync(this FileInfo fi, string text, Encoding enc = null){
			if(!fi.Directory.Exists){
				fi.Directory.Create();
			}
			var buf = Encoding.UTF8.GetBytes(text);
			using (var fs = new FileStream(fi.FullName, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true)){
				await fs.WriteAsync(buf, 0, buf.Length).ConfigureAwait(false);
			};
		}
	}
}