using Enable_Now_Konnektor.src.misc;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Enable_Now_Konnektor.src.enable_now
{
    public class Element : ICloneable
    {
        public string Id { get; set; }
        public string Class { get; set; }
        public Dictionary<string, List<string>> Fields { get; set; } = new Dictionary<string, List<string>>();

        /// <summary>
        /// Hashwert der Felder, um zu überprüfen, ob ein Element veraltet ist
        /// </summary>
        public int Hash { get; set; }
        public string[] ChildrenIds { get; set; }
        public string[] AttachementNames { get; set; }

        public const string Group = "GR";
        public const string Project = "PR";
        public const string Slide = "SL";

        private readonly ILog _log = LogManager.GetLogger(typeof(Element));

        /// <summary>
        /// Erstellt ein neues Objekt, das einem Element in Enable Now entspricht.
        /// </summary>
        /// <param name="id">Die ID es Enable Now Elements.</param>
        /// <exception cref="ArgumentException">Wirft eine Ausnahme, falls die ID ungültig ist.</exception>
        public Element(string id)
        {
            if (!Validator.Validate(id, Validator.EnableNowIdPattern))
            {
                _log.Error($"Die ID {id} des Enable Now Elements entspricht keinem gültigen Muster.");
                throw new ArgumentException($"Die ID {id} des Enable Now Elements entspricht keinem gültigen Muster.");
            }
            Id = id;
            Class = Id.Split('_')[0];
        }

        public void AddValues(string name, List<string> values)
        {
            AddValues(name, values.ToArray());
        }

        public void AddValues(string name, params string[] values)
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

        public void ReplaceValues(string name, List<string> values)
        {
            ReplaceValues(name, values.ToArray());
        }

        public void ReplaceValues(string name, params string[] values)
        {
            if (Fields.ContainsKey(name))
            {
                Fields[name].Clear();

            }
            AddValues(name, values);
        }

        public List<string> GetValues(string name)
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
    }
}
