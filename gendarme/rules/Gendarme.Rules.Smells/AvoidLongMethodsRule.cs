//
// Gendarme.Rules.Smells.AvoidLongMethodsRule class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007 Néstor Salceda
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Smells {

	public class AvoidLongMethodsRule : IMethodRule {
		private int maxInstructions = 170;
		static Hashtable typeMethodDictionary;

		static AvoidLongMethodsRule ()
		{
			typeMethodDictionary = new Hashtable ();
			typeMethodDictionary.Add ("Gtk.Bin", "Build");
			typeMethodDictionary.Add ("Gtk.Window", "Build");
			typeMethodDictionary.Add ("Gtk.Dialog", "Build");
			typeMethodDictionary.Add ("System.Windows.Forms.Form", "InitializeComponent");
		}

		public int MaxInstructions {
			get {
				return maxInstructions;
			}
			set {
				maxInstructions = value;
			}
		}

		private static bool IsAutogeneratedByTools (MethodDefinition method)
		{
			if (method.Parameters.Count != 0)
				return false;

			if (method.DeclaringType is TypeDefinition) {
				TypeDefinition type = (TypeDefinition) method.DeclaringType;
				if (typeMethodDictionary.ContainsKey (type.BaseType.FullName))
					return (String.Compare (method.Name, (string) typeMethodDictionary[type.BaseType.FullName]) == 0);
			}
			return false;
		}

		private static bool IsStaticConstructor (MethodDefinition method)
		{
			return method.Name == MethodDefinition.Cctor;
		}

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			// rule does not apply if method as no code (e.g. abstract, p/invoke)
			if (!method.HasBody)
				return runner.RuleSuccess;

			// rule does not apply to code outside the developer's control
			if (method.IsGeneratedCode ())
				return runner.RuleSuccess;

			// rule does not apply to autogenerated code from some
			// tools
			if (IsAutogeneratedByTools (method))
				return runner.RuleSuccess;
			
			// Extracting static methods from static constructors really
			// makes sense? 	
			if (IsStaticConstructor (method))
				return runner.RuleSuccess;

			// rule applies!

			// success if the instruction count is below the defined threshold
			if (method.Body.Instructions.Count <= MaxInstructions)
				return runner.RuleSuccess;

			Location location = new Location (method);
			Message message = new Message ("The method is too long.", location, MessageType.Error);
			return new MessageCollection (message);
		}
	}
}
