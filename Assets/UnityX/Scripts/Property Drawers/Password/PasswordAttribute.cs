// https://github.com/anchan828/property-drawer-collection
// Copyright (C) 2014 Keigo Ando
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;

public class PasswordAttribute : PropertyAttribute {

	public char mask = '*';
	public bool useMask = true;
	public int minLength = 0;
	public int maxLength = 2147483647;

	public PasswordAttribute () {}

	public PasswordAttribute (int minLength, int maxLength) {
		this.minLength = minLength;
		this.maxLength = maxLength;
	}
	
	public PasswordAttribute (int minLength, bool useMask) {
		this.useMask = useMask;
		this.minLength = minLength;
	}
	
	public PasswordAttribute (int minLength, int maxLength, bool useMask) {
		this.useMask = useMask;
		this.minLength = minLength;
		this.maxLength = maxLength;
	}
}