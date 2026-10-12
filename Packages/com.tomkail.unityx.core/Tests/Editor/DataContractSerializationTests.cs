using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace UnityX.Tests {
    // UnityX types declare their JSON shape with the BCL [DataContract]/[DataMember] attributes, so they serialize
    // correctly with Newtonsoft (which honours them) without UnityX depending on it. [DataContract] is opt-in, so the
    // risk is a field added later without [DataMember] - silently missing from every save. These tests check, for
    // every [DataContract] type in a UnityX* assembly, that the JSON contract is exactly the set of fields Unity
    // serializes, and that the type can be constructed on load.
    public class DataContractSerializationTests {
        const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        static IEnumerable<Type> DataContractTypes () {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                var name = asm.GetName().Name;
                if (!name.StartsWith("UnityX") || name.EndsWith(".Tests")) continue;
                Type[] types;
                try { types = asm.GetTypes(); } catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
                foreach (var t in types.Where(t => t.IsDefined(typeof(DataContractAttribute), false)).OrderBy(t => t.FullName))
                    yield return t;
            }
        }

        // One case per type, named "Check(Type)" so a failure says both which check and which type.
        static IEnumerable<TestCaseData> Cases (string check) => DataContractTypes().Select(t => new TestCaseData(t).SetName($"{check}({t.FullName})"));
        static IEnumerable<TestCaseData> FieldCases () => Cases(nameof(JsonMembersMatchUnitySerializedFields));
        static IEnumerable<TestCaseData> ConstructorCases () => Cases(nameof(CanBeConstructedOnLoad));

        // Generic definitions are checked through a closed instance; try type arguments until one satisfies the constraints.
        static Type Close (Type t) {
            if (!t.IsGenericTypeDefinition) return t;
            foreach (var arg in new[] { typeof(DayOfWeek), typeof(int), typeof(string) }) {
                try { return t.MakeGenericType(t.GetGenericArguments().Select(_ => arg).ToArray()); } catch (ArgumentException) { }
            }
            Assert.Inconclusive($"Couldn't close generic type {t}");
            return null;
        }

        // Mirrors Unity's rules closely enough for this check: instance fields that are public or [SerializeField],
        // not [NonSerialized], readonly or const. Delegates and UnityEngine.Object references are left out - Unity
        // handles references itself, and neither belongs in JSON.
        static HashSet<string> UnitySerializedFields (Type t) {
            var names = new HashSet<string>();
            for (var c = t; c != null && c != typeof(object) && c != typeof(ValueType); c = c.BaseType) {
                foreach (var f in c.GetFields(Fields)) {
                    if (f.IsNotSerialized || f.IsInitOnly || f.IsLiteral) continue;
                    if (!f.IsPublic && !f.IsDefined(typeof(UnityEngine.SerializeField), true)) continue;
                    if (typeof(Delegate).IsAssignableFrom(f.FieldType) || typeof(UnityEngine.Object).IsAssignableFrom(f.FieldType)) continue;
                    names.Add(f.Name);
                }
            }
            return names;
        }

        static JsonObjectContract Contract (Type t) => (JsonObjectContract)new DefaultContractResolver().ResolveContract(t);

        [Test, TestCaseSource(nameof(FieldCases))]
        public void JsonMembersMatchUnitySerializedFields (Type type) {
            var t = Close(type);
            var json = Contract(t).Properties.Where(p => !p.Ignored).ToList();

            var properties = json.Where(p => t.GetProperty(p.UnderlyingName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null).Select(p => p.UnderlyingName).ToList();
            Assert.IsEmpty(properties, $"{t.Name}: [DataMember] should be on fields, not properties (computed properties can recurse or duplicate data)");

            var unity = UnitySerializedFields(t);
            var jsonFields = new HashSet<string>(json.Select(p => p.UnderlyingName));
            var missing = unity.Except(jsonFields).OrderBy(n => n).ToList();
            var extra = jsonFields.Except(unity).OrderBy(n => n).ToList();
            Assert.IsEmpty(missing, $"{t.Name}: Unity serializes these fields but they have no [DataMember], so JSON drops them");
            Assert.IsEmpty(extra, $"{t.Name}: these [DataMember] fields aren't serialized by Unity (add [SerializeField], or drop the [DataMember])");
        }

        [Test, TestCaseSource(nameof(ConstructorCases))]
        public void CanBeConstructedOnLoad (Type type) {
            var t = Close(type);
            if (t.IsValueType) return;
            var c = Contract(t);
            Assert.IsTrue(c.DefaultCreator != null || c.OverrideCreator != null,
                $"{t.Name}: no parameterless constructor (or [JsonConstructor]), so Newtonsoft can't create it when loading");
        }

        [Test]
        public void FindsAnnotatedTypes () {
            Assert.IsNotEmpty(DataContractTypes().ToList(), "No [DataContract] types found in UnityX assemblies");
        }
    }
}
