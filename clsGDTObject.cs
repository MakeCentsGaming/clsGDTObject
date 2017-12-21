using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace MakeCents
{
   /// <summary>
   /// <para>Creates an object including lists of GDF objects</para>
   /// <para>//Includes ability to report progress </para>
   /// <para>GDF object includes all asset information including which gdt it came from</para>
   /// </summary>
   /// 
   class GDT
   {
      public Action<double> ProgressBar;
      public Action<string> MessageBar;
      public List<GDF> gdfs;      

      /// <summary>
      /// Override to initialize gdfs as new list
      /// </summary>
      public GDT()
      {
         gdfs = new List<GDF>();
      }
      
      public void ReadEachGDT(List<string> fileNames)
      {
         foreach (string cb in fileNames)
         {
            SplitGDT(cb, fileNames.Count);
         }
      }
      /// <summary>
      /// Splits a gdt into GDFs, assigned to GDT.gdfs
      /// </summary>
      /// <param name="fileName">The full path to the gdt</param>
      /// <param name="fcount">The number of files that will equal 90 percent of the effort</param>
      public void SplitGDT(string fileName, int fcount = 1)
      {
         string gdttext = GetStringFromFile(fileName);
         if (gdttext == string.Empty) return;
         string[] allgdfs = gdttext.Split('{');
         StringBuilder sb = new StringBuilder();
         double inc = (90.0 / allgdfs.Count()/fcount);
         foreach (string i in allgdfs)
         {
            if(ProgressBar!=null)
            {
               ProgressBar(inc);
            }
            
            sb = SeperateGDT(sb, i, fileName);
         }
      }
      /// <summary>
      /// Error handling for getting all text from file
      /// </summary>
      /// <param name="fileName">Full name of a gdt file</param>
      /// <returns></returns>
      private string GetStringFromFile(string fileName)
      {
         if (!fileName.Contains(".gdt"))
         {
            MessageBox.Show(fileName + " is not a gdt file.\n\nI only edit gdt files.", "Not a gdt file",
               MessageBoxButton.OK, MessageBoxImage.Error);
            return "";
         }
         try
         {
            return File.ReadAllText(fileName);
         }
         catch(FileNotFoundException)
         {
            MessageBox.Show(fileName + " not found", "File not found",
               MessageBoxButton.OK, MessageBoxImage.Error);
            return "";
         }
         catch(FieldAccessException)
         {
            MessageBox.Show(fileName + " is being used by another process", "File in use",
               MessageBoxButton.OK, MessageBoxImage.Error);
            return "";
         }
         catch
         {
            MessageBox.Show(fileName + " had an error.\n\nNot enough information to know why.\n\nSorry :'(", "Error",
               MessageBoxButton.OK, MessageBoxImage.Error);
            return "";
         }
      }

      /// <summary>
      /// Splits the split again but by } and determines to start over the stringbuilder or to continue it
      /// </summary>
      /// <param name="sb">The current gdfs stringbuilder</param>
      /// <param name="i">the line in the gdf</param>
      /// <param name="fileName">The name of the gdt to add to the gdf property</param>
      private StringBuilder SeperateGDT(StringBuilder sb, string i, string fileName)
      {
         
         if (i.Contains("}"))
         {            
            string[] lsplit = i.Split('}');
            sb.AppendLine(lsplit[0]);
            GDF mgdf = new GDF();

            //mgdf.PassBackMessage = ReportCurrentMessage;//background worker stuff taken out
            //Console.WriteLine(sb.ToString());
            if (MessageBar != null)
            {
               try
               {
                  MessageBar(sb.ToString().Trim().Split('\n')[0].Split('\"')[1]);
               }
               catch
               {
                  //either the length wasn't long enough, or didn't include new line or "
               }
            }
            mgdf.GatherGDFS(sb.ToString(), fileName);
            this.gdfs.Add(mgdf);

            sb = new StringBuilder();
            sb.AppendLine(lsplit[1]);
            return sb;
         }
         sb.AppendLine(i);         
         return sb;
      }
 
      /// <summary>
      /// Checks if the syntax could be correct by counting opening, closing, and quotes
      /// </summary>
      /// 
      private bool GoodSyntaxCount(string file)
      {
         string gdttext = GetStringFromFile(file);
         if (gdttext == string.Empty) return false;
         int countsb = gdttext.Count(f => f == '{');
         int counteb = gdttext.Count(f => f == '}');
         int countsp = gdttext.Count(f => f == '(');
         int countep = gdttext.Count(f => f == ')');
         int countq = gdttext.Count(f => f == '"');
         return (countsb == counteb && countsp == countep && countq % 2 == 0);
      }

      /// <summary>
      /// <para>ModifyValueForKeyForEach(key, value, [add]);</para>
      /// <para>Update the value for a kvp or add it if it doesn't exists</para>
      /// </summary>
      /// <param name="key">The key to update the value for</param>
      /// <param name="nvalue">The new value for this key</param>
      /// <param name="add">[Optional]Add kvp if it don't exist</param>
      /// 
      public void ModifyValueForKeyForEach(string key, string nvalue, bool add = false)
      {
         if (gdfs != null)
         {
            foreach (GDF gdf in gdfs)
            {
               gdf.ModifyValueForKey(key, nvalue, add);
            }
         }
      }

      /// <summary>
      /// Edit a value for a specific key in all assets
      /// </summary>
      /// <param name="key">Key to replace value for</param>
      /// <param name="start">Keep up to and including this part of the value</param>
      /// <param name="end">The part of the string to keep at the end</param>
      /// <param name="add">[Optional] Add this in between to add to the string, nothing will just remove it</param>
      public void InsertValueForEach(string key, string start, string end, string add="")
      {
         if (gdfs != null)
         {
            foreach (GDF gdf in gdfs)
            {
               gdf.EditValue(key, start, end, add);
            }
         }
      }

      /// <summary>
      /// Replace a value for a specific string for each asset
      /// </summary>
      /// <param name="key">The key to replace the value string for</param>
      /// <param name="replace">What you want to replace</param>
      /// <param name="replacewith">What you will replace it with if it exists</param>
      public void ReplaceEachValue(string key, string replace, string replacewith)
      {
         if (gdfs != null)
         {
            foreach (GDF gdf in gdfs)
            {
               gdf.ReplaceValue(key, replace, replacewith);
            }
         }
      }
      /// <summary>
      /// Add a prefix to each asset in the list of assets
      /// </summary>
      /// <param name="prefix">The prefix to add to the name</param>
      /// <param name="duplicate">If the prefix is on the name do you want to add it again? True prevents accidental duplicates but keeps adding prefix</param>
      public void AddPrefixToEachName(string prefix, bool duplicate = false)
      {
         if (gdfs!=null)
         {
            foreach(GDF gdf in gdfs)
            {
               gdf.AddPrefixToName(prefix, duplicate);
            }
         }
      }
      /// <summary>
      /// Add a prefix to each asset in the list of assets
      /// </summary>
      /// <param name="suffix">The prefix to add to the name</param>
      /// <param name="duplicate">If the prefix is on the name do you want to add it again? True prevents accidental duplicates but keeps adding suffix</param>
      public void AddSuffixToEachName(string suffix, bool duplicate = false)
      {
         if (gdfs != null)
         {
            foreach (GDF gdf in gdfs)
            {
               gdf.AddSuffixToName(suffix, duplicate);
            }
         }
      }
      /// <summary>
      /// var x = GetAssetsOfType("material.gdf");
      /// </summary>
      /// <param name="type">[Optional] The type of asset list to return, includes .gdf on the end or it wll add it</param>
      /// <returns>IEnumberable of clsGDFObject</returns>
      public IEnumerable<GDF> GetAssetsOfType(string type = "*")
      {
         if (gdfs == null) return new List<GDF>();
         if (type != "*" && !type.Trim().EndsWith(".gdf")) type = type + ".gdf";
         if (type == "*")
         {
            return gdfs;
         }
         var types = gdfs.Where(t => t.Type == type);
         return types;
      }
      /// <summary>
      /// Get the entire gdt for a specific name
      /// </summary>
      /// <param name="mygdt">The gdt you added to read</param>
      /// <param name="duplicates">[Optional] include duplicates or not</param>
      /// <returns>String of entire GDT complete formatted</returns>
      public string GetGDT(string mygdt, bool duplicates = false)
      {
         if (gdfs == null || mygdt == "" || mygdt == null) return "";
         var mygdfs = gdfs.Where(g => g.gdt == mygdt);
         StringBuilder sb = new StringBuilder();
         sb.AppendLine("{");
         sb.AppendLine(CollectGDT(mygdfs, duplicates));
         sb.AppendLine("}");
         return sb.ToString();
      }
      /// <summary>
      /// Gathers one gdt together
      /// </summary>
      /// <param name="mygdfs">An IEnumerable of GDF objects</param>
      /// <param name="duplicates">Include duplicates or not</param>
      /// <returns></returns>
      private string CollectGDT(IEnumerable<GDF> mygdfs, bool duplicates)
      {
         if (gdfs != null)
         {
            List<string> names = new List<string>();
            StringBuilder sb = new StringBuilder();

            try
            {
               foreach (GDF gdf in mygdfs)
               {
                  if (!duplicates && names.Contains(gdf.Name + gdf.Type)) continue;
                  names.Add(gdf.Name + gdf.Type);
                  sb.AppendLine(string.Format("{0}", gdf.GetNameAndType()));
                  sb.AppendLine("\t{");
                  sb.AppendLine(string.Format("{0}", gdf.GetAllKVPs()));
                  sb.AppendLine("\t}");
               }
            }
            catch
            {
               //operation was cancelled
               //MessageBox.Show(ex.Message);
            }
            return sb.ToString();
         }
         return "";         
      }
      /// <summary>
      /// Combines all gdts into one gdt string
      /// </summary>
      /// <param name="duplicates">[Optional] include duplicates or not</param>
      /// <returns></returns>
      public string GetCombinedGDT(bool duplicates = false)
      {
         if (gdfs != null)
         {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine(CollectGDT(gdfs, duplicates));
            sb.AppendLine("}");
            return sb.ToString();
         }
         return "";
      }
   }
   /// <summary>
   /// <para>GDF object includes all asset information including which gdt it came from</para>
   /// </summary>
   public class GDF
   {
      //public Action<string> PassBackMessage;
      /// <summary>
      /// Type is the name of the gdt, material, xmodel... etc
      /// </summary>
      public string Type { get; set; }
      /// <summary>
      /// Name is the name of the asset
      /// </summary>
      public string Name { get; set; }
      /// <summary>
      /// If it is inherited or not
      /// </summary>
      public bool inherited;
      /// <summary>
      /// The key value pairs in this gdf
      /// </summary>
      public Dictionary<string, string> Kvps { get; set; }
      /// <summary>
      /// The order of kvps in this gdf 
      /// </summary>
      public List<string> Kvporder { get; set; }
      /// <summary>
      /// The path of the gdt
      /// </summary>
      public string gdt { get; set; }

      /// <summary>
      /// Takes a gdf, from the starting "name" to the ending } and creates
      /// a dictionary of kvps, name, and type as well as tracks order with a list
      /// Requires a good gdt to be passsed
      /// </summary>
      /// <param name="gdf">String from "name" to }</param>
      /// <param name="gdt">Name of this gdfs gdt for gathering later</param>
      public void GatherGDFS(string gdf, string gdt)
      {
         gdf = gdf.Trim();
         this.gdt = gdt;
         string[] lines = gdf.Split('\n');
         Kvps = new Dictionary<string, string>();
         //Console.WriteLine(lines[0]);
         if (!GoodLine(lines[0])) return;

         this.Name = lines[0].Split('"')[1];
         this.Type = lines[0].Split('"')[3];
         if(!lines[0].Contains("(") && lines[0].Contains("["))
         {
            inherited = true;
         }
         else
         {
            inherited = false;
         }
         this.Kvporder = new List<string>();

         //pass back what we are working on
         //PassBackMessage(this.Name);

         for (int i = 1; i < lines.Length; i++)
         {
            if (!lines[i].Contains("\"")) continue;
            if (lines[i].Split('"').Length < 4) continue;
            string key = lines[i].Split('"')[1];

            //Console.WriteLine(lines[i]);
            //Thread.Sleep(1);

            string value = lines[i].Split('"')[3];

            Kvps.Add(key, value);
            this.Kvporder.Add(key);
         }
      }
      /// <summary>
      /// Returns if the line is ok to split by " and has a key and value
      /// </summary>
      /// <param name="v">The line to split</param>
      /// <returns>if the line is good to split and read the 3rd index</returns>
      private bool GoodLine(string v)
      {
         //Console.WriteLine(v);
         if (!v.Contains("\"")) return false;
         if (v.Split('\"').Length < 4) return false;
         return true;
      }
      /// <summary>
      /// Get the name of a asset
      /// </summary>
      /// <returns>A formatted asset name</returns>
      public string GetNameAndType()
      {
         
         return GDTUtil.AssetNameAndType(this.Name, this.Type, this.inherited);
      }
      /// <summary>
      /// <para>gdf.ModifyValueForKey(key, value, [add]);</para>
      /// <para>Update the value for a kvp or add it if it doesn't exists</para>
      /// </summary>
      /// <param name="key">The key to update the value for</param>
      /// <param name="nvalue">The new value for this key</param>
      /// <param name="add">[Optional]Add kvp if it don't exist</param>
      /// 
      public void ModifyValueForKey(string key, string nvalue, bool add = false)
      {
         if (this.GetValueForKey(key) == null)
         {
            if (add && !this.inherited)
               this.Kvps.Add(key, nvalue);
         }
         else
         {
            this.Kvps[key] = nvalue;
         }
      }

      /// <summary>
      /// <para>gdf.UpdateValueFor(key, value, false);</para>
      /// <para>Update a value for a key, but only if the value is nothing, unless onlyifempty == true</para>
      /// </summary>
      /// <param name="key">the key to update the value for</param>
      /// <param name="value">The new value for the key</param>
      /// <param name="onlyifempty">[Optional] Only update for keys that value == ""</param>
      /// <param name="addifmissing">[Optional] Add kvps that do not exist</param>
      /// 
      public void UpdateValueFor(string key, string value, bool onlyifempty = false, bool addifmissing = false)
      {
         switch (this.GetValueForKey(key))
         {
            case null:
               if (addifmissing)
               {
                  this.ModifyValueForKey(key, value, true);
               }
               break;
            case "":
               this.ModifyValueForKey(key, value);
               break;
            default:
               if (!onlyifempty)
               {
                  this.ModifyValueForKey(key, value);
               }
               break;
         }
      }
      /// <summary>
      /// Get the body of an asset, or all the kvps
      /// </summary>
      /// <returns>All kvps in a nice string format</returns>
      /// 
      public string GetAllKVPs()
      {
         return string.Join("\n", this.Kvps.Select(x => GDTUtil.KVP(x.Key, x.Value)).ToArray());
      }
      /// <summary>
      /// Get the entire asset/gdf
      /// </summary>
      /// <returns>The asset complete formatted</returns>
      public string GetAsset()
      {
         StringBuilder sb = new StringBuilder();
         sb.AppendLine(string.Format("{0}", this.GetNameAndType()));
         sb.AppendLine("\t{");
         sb.AppendLine(string.Format("{0}", this.GetAllKVPs()));
         sb.AppendLine("\t}");
         return sb.ToString();
      }      
      /// <summary>
      /// Get the value of a key
      /// </summary>
      /// <param name="key">key to look up in this asset</param>
      /// <returns>the value for a kvp or null if the key does not exist</returns>
      public string GetValueForKey(string key)
      {
         if (this.Kvps.Keys.Contains(key))
         {
            return this.Kvps[key];
         }
         return null;
      }

      /// <summary>
      /// Adds a prefix to the name and if inherited adds to the inheritied name
      /// </summary>
      /// <param name="prefix">String to add as prefix</param>
      /// <param name="duplicate">If it is already prefixed, do you want to duplicate it?</param>
      public void AddPrefixToName(string prefix, bool duplicate = false)
      {
         GDTUtil.AddPrefixToName(this, prefix, duplicate);
      }

      /// <summary>
      /// Adds a suffix to the name and if inherited adds to the inheritied name
      /// </summary>
      /// <param name="suffix">String to add as suffix</param>
      /// <param name="duplicate">If it is already prefixed, do you want to duplicate it?</param>
      public void AddSuffixToName(string suffix, bool duplicate = false)
      {
         GDTUtil.AddSuffixToName(this, suffix, duplicate);
      }



      /// <summary>
      /// Replace instances of a string for a specific key
      /// </summary>
      /// <param name="key">The key to replace the value string for</param>
      /// <param name="replace">What you want to replace</param>
      /// <param name="replacewith">What you will replace it with if it exists</param>
      public void ReplaceValue(string key, string replace, string replacewith)
      {
         string value;
         if (this.Kvps != null)
         {
            if (this.Kvps.Keys.Contains(key))
            {
               value = this.Kvps[key];
               if(value.Contains(replace))
               {
                  this.Kvps[key] = value.Replace(replace, replacewith);
               }
            }
         }
      }

      /// <summary>
      /// Edit values for specific keys based on start and ending. 
      /// </summary>
      /// <param name="key">Key to replace value for</param>
      /// <param name="start">Keep up to and including this part of the value</param>
      /// <param name="end">The part of the string to keep at the end</param>
      /// <param name="add">[Optional] Add this in between to add to the string, nothing will just remove it</param>
      public void EditValue(string key, string start, string end, string add="")
      {
         string value;
         string mstart = "";
         string mend = "";
         string rema;
         StringBuilder sb;
         if (this.Kvps!=null)
         {
            if(this.Kvps.Keys.Contains(key))
            {
               value = this.Kvps[key];

               if(value.Contains(start) && start!="")
               {
                  mstart = value.Substring(0,value.IndexOf(start) + start.Length);
               }
               else
               {
                  return;
               }
               rema = value.Substring(mstart.Length);
               //Console.WriteLine("mstart: " + mstart);
               //Console.WriteLine("rema: " + rema);
               if(rema.Contains(end)&end!="")
               {
                  mend = rema.Substring(rema.IndexOf(end));
               }
               else
               {
                  return;
               }
               //Console.WriteLine("end: " + mend);
               sb = new StringBuilder();
               sb.Append(mstart);
               sb.Append(add);
               sb.Append(mend);
               this.Kvps[key] = sb.ToString().Replace("\\\\\\\\", "\\\\");
            }
         }
      }
   }

   /// <summary>
   /// Static utilities for common things like formatting strings and renaming assets
   /// </summary>
   static class GDTUtil
   {
      /// <summary>
      /// formats for kvp
      /// </summary>
      /// <param name="key">the key for the attribute</param>
      /// <param name="value">the value for the attribute</param>
      /// <returns></returns>
      public static string KVP(string key, string value)
      {
         return string.Format("\t\t\"{0}\"", key) + string.Format(" \"{0}\"", value);
      }
      /// <summary>
      /// Pass a gdf object to get name and type string
      /// </summary>
      /// <param name="g">GDF object</param>
      /// <returns></returns>
      public static string AssetNameAndType(GDF g)
      {
         return AssetNameAndType(g.Name, g.Type, g.inherited);
      }
      /// <summary>
      /// formats for name and type of gdf
      /// </summary>
      /// <param name="name">The name of the asset/gdf</param>
      /// <param name="type">The type of asset, such as material.gdf</param>
      /// <param name="inherited">[Optional] If this asset inherits from another asset</param>
      /// <returns></returns>
      public static string AssetNameAndType(string name, string type, bool inherited=false)
      {
         //Console.WriteLine(name + " " + inherited);
         if (inherited)
         {
            return string.Format("\t\"{0}\"", name) + string.Format(" [ \"{0}\" ]", type);
         }
         return string.Format("\t\"{0}\"", name) + string.Format(" ( \"{0}\" )", type);
      }

      /// <summary>
      /// Formats an inherted asset
      /// </summary>
      /// <param name="name"></param>
      /// <param name="type"></param>
      /// <returns></returns>
      public static string Inherited(string name, string type)
      {
         return string.Format("\t\"{0}\"", name) + string.Format(", \"{0}\"", type);
      }

      /// <summary>
      /// Adds a prefix to the name and if inherited adds to the inheritied name
      /// </summary>
      /// <param name="gdf">GDF object</param>
      /// <param name="prefix">String to add as prefix</param>
      /// <param name="duplicate">If it is already prefixed, do you want to duplicate it?</param>
      public static void AddPrefixToName(GDF gdf, string prefix, bool duplicate = false)
      {
         gdf.Name = AddToNameOrType(gdf.Name.StartsWith(prefix), duplicate, gdf.Name, prefix);
         if (gdf.inherited)
         {
            gdf.Type = AddToNameOrType(gdf.Type.StartsWith(prefix), duplicate, gdf.Type, prefix);           
         }
      }

      /// <summary>
      /// returns modified name with prefix and or suffix
      /// </summary>
      /// <param name="v">Where or not the prefix or suffix is already on the name</param>
      /// <param name="duplicate">If you want to duplicate even if it is already on the name</param>
      /// <param name="name">The core name to add prefix or suffix to</param>
      /// <param name="prefix">What to add to the beginning of the name</param>
      /// <param name="suffix">What to add to the end of the name</param>
      /// <returns></returns>
      private static string AddToNameOrType(bool v, bool duplicate, string name, string prefix, string suffix = "")
      {
         if(!v || duplicate)
         {
            return prefix + name + suffix;
         }
         return name;
      }
      /// <summary>
      /// Adds a suffix to the name and if inherited adds to the inheritied name
      /// </summary>
      /// <param name="gdf">GDF object</param>
      /// <param name="suffix">String to add as suffix</param>
      /// <param name="duplicate">If it is already prefixed, do you want to duplicate it?</param>
      public static void AddSuffixToName(GDF gdf, string suffix, bool duplicate = false)
      {
         gdf.Name = AddToNameOrType(gdf.Name.EndsWith(suffix), duplicate, gdf.Name, "", suffix);

         if (gdf.inherited)
         {
            gdf.Type = AddToNameOrType(gdf.Type.StartsWith(suffix), duplicate, gdf.Type, "",suffix);
         }
      }
   }
}
