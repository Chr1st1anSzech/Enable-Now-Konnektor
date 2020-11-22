using Enable_Now_Konnektor.src.misc;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Enable_Now_Konnektor.src.enable_now
{
    internal class Element : ICloneable
    {
        internal string Id { get; set; }
        internal string Class { get; set; }
        internal Dictionary<string, List<string>> Fields { get; set; } = new Dictionary<string, List<string>>();

        /// <summary>
        /// Hashwert der Felder, um zu überprüfen, ob ein Element veraltet ist
        /// </summary>
        internal string Hash { get; set; }
        internal string[] ChildrenIds { get; set; }
        internal string[] AttachementNames { get; set; }

        internal const string Group = "GR";
        internal const string Project = "PR";
        internal const string Slide = "SL";
        internal const string Media = "M";

        private readonly ILog _log = LogManager.GetLogger(typeof(Element));

        /// <summary>
        /// Erstellt ein neues Objekt, das einem Element in Enable Now entspricht.
        /// </summary>
        /// <param name="id">Die ID es Enable Now Elements.</param>
        /// <exception cref="ArgumentException">Wirft eine Ausnahme, falls die ID ungültig ist.</exception>
        internal Element(string id)
        {
            if (!Validator.Validate(id, Validator.EnableNowIdPattern))
            {
                string message = Util.GetFormattedResource("ElementMessage01", id);
                _log.Error(message);
                throw new ArgumentException(message);
            }
            Id = id;
            Class = Id.Split('_')[0];
        }

        internal void AddValues(string name, List<string> values)
        {
            AddValues(name, values.ToArray());
        }

        internal void AddValues(string name, params string[] values)
        {
            if (name == null)
            {
                return;
            }
            if (Fields.ContainsKey(name))
            {
                AddValuesToField(name, values);
            }
            else
            {
                AddNewField(name, values);
            }
        }



        private void AddValuesToField(string name, params string[] values)
        {
            if (values != null && values.Length > 0)
            {
                AddValuesToList(name, values);
            }
        }

        /// <summary>
        /// Füge die Werte hinzu, die noch nicht in der Liste und nicht leer sind.
        /// </summary>
        /// <param name="name">Name des Feldes</param>
        /// <param name="values">Werte, die hinzugefügt werden sollen</param>
        private void AddValuesToList(string name, string[] values)
        {
            Fields[name].AddRange(values.Where(value => !Fields[name].Contains(value) && !string.IsNullOrWhiteSpace(value)));
        }

        private void AddNewField(string name, params string[] values)
        {
            if (values != null && values.Length > 0)
            {
                List<string> list = new List<string>();
                Fields.Add(name, list);
                AddValuesToList(name, values);
            }
        }

        internal void ReplaceValues(string name, List<string> values)
        {
            ReplaceValues(name, values.ToArray());
        }

        internal void ReplaceValues(string name, params string[] values)
        {
            if (Fields.ContainsKey(name))
            {
                Fields[name].Clear();

            }
            AddValues(name, values);
        }

        internal List<string> GetValues(string name)
        {
            if (Fields.ContainsKey(name))
            {
                return Fields[name];
            }

            return null;
        }

        public override string ToString()
        {
            return Id;
        }

        public object Clone()
        {
            Element copy = MemberwiseClone() as Element;
            copy.AttachementNames = AttachementNames.Clone() as string[];
            copy.ChildrenIds = ChildrenIds.Clone() as string[];
            copy.Fields = Fields.ToDictionary(entry => entry.Key,
                                               entry => new List<string>(entry.Value) );
            copy.Id = Id.Clone() as string ;
            copy.Class = Class.Clone() as string ;
            return copy;
        }

        internal string GenerateHashCode()
        {
            var values = Fields.SelectMany(field => field.Value);
            string concatenatedString = string.Join(",", values);

            var hasher = new SHA1Managed();
            byte[] byteArray = Encoding.UTF8.GetBytes(concatenatedString);
            byte[] hashByteArray = hasher.ComputeHash(byteArray);
            string res = "";
            foreach (var b in hashByteArray)
            {
                res += b.ToString();
            }

            return res;
        }
    }
}
