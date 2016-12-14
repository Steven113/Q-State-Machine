//From: http://www.technologicalutopia.com/sourcecode/xnageometry/vector3.cs.htm

/*
Vector3.cs in C//#
	Free, commercially distributable, modifiable, open source code. This class is part of the XnaGeometry library, a 3d library. To download the entire XnaGeometry library, click here. XnaGeometry uses the same function names as XNA so you can use the Microsoft XNA documentation. XnaGeometry will allow you to decouple your calculations from the renderer, and uses floats for precision. Monogame has rendering support. 
		
		
		//#region License

MIT License
Copyright Â© 2006 The Mono.Xna Team

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE.x
SOFTWARE.
*/
		//#endregion License
		
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace XnaGeometry
{
	[Serializable]
	public struct Vector3Custom : IEquatable<Vector3Custom>
	{
		//#region Private Fields
		
		public static  Vector3Custom zero = new Vector3Custom(0f, 0f, 0f);
		public static  Vector3Custom one = new Vector3Custom(1f, 1f, 1f);
		public static  Vector3Custom unitX = new Vector3Custom(1f, 0f, 0f);
		public static  Vector3Custom unitY = new Vector3Custom(0f, 1f, 0f);
		public static  Vector3Custom unitZ = new Vector3Custom(0f, 0f, 1f);
		public static  Vector3Custom up = new Vector3Custom(0f, 1f, 0f);
		public static  Vector3Custom down = new Vector3Custom(0f, -1f, 0f);
		public static  Vector3Custom right = new Vector3Custom(1f, 0f, 0f);
		public static Vector3Custom left = new Vector3Custom(-1f, 0f, 0f);
		public static Vector3Custom forward = new Vector3Custom(0f, 0f, -1f);
		public static Vector3Custom backward = new Vector3Custom(0f, 0f, 1f);
		
		//#endregion Private Fields
		
		
		//#region Public Fields
		
		public float x;
		public float y;
		public float z;
		
		//#endregion Public Fields
		
		
		//#region Properties
		
		public static Vector3Custom Zero
		{
			get { return zero; }
		}
		
		public static Vector3Custom One
		{
			get { return one; }
		}
		
		public static Vector3Custom UnitX
		{
			get { return unitX; }
		}
		
		public static Vector3Custom UnitY
		{
			get { return unitY; }
		}
		
		public static Vector3Custom UnitZ
		{
			get { return unitZ; }
		}
		
		public static Vector3Custom Up
		{
			get { return up; }
		}
		
		public static Vector3Custom Down
		{
			get { return down; }
		}
		
		public static Vector3Custom Right
		{
			get { return right; }
		}
		
		public static Vector3Custom Left
		{
			get { return left; }
		}
		
		public static Vector3Custom Forward
		{
			get { return forward; }
		}
		
		public static Vector3Custom Backward
		{
			get { return backward; }
		}
		
		//#endregion Properties
		
		
		//#region Constructors
		
		public Vector3Custom(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
		
		
		public Vector3Custom(float value)
		{
			this.x = value;
			this.y = value;
			this.z = value;
		}
		
		

		
		
		//#endregion Constructors
		
		
		//#region Public Methods
		
		public static Vector3Custom Add(Vector3Custom value1, Vector3Custom value2)
		{
			value1.x += value2.x;
			value1.y += value2.y;
			value1.z += value2.z;
			return value1;
		}
		
		public static void Add(ref Vector3Custom value1, ref Vector3Custom value2, out Vector3Custom result)
		{
			result.x = value1.x + value2.x;
			result.y = value1.y + value2.y;
			result.z = value1.z + value2.z;
		}
		

		
	
		

		
		public static Vector3Custom Cross(Vector3Custom vector1, Vector3Custom vector2)
		{
			Cross(ref vector1, ref vector2, out vector1);
			return vector1;
		}
		
		public static void Cross(ref Vector3Custom vector1, ref Vector3Custom vector2, out Vector3Custom result)
		{
			result = new Vector3Custom(vector1.y * vector2.z - vector2.y * vector1.z,
			                     -(vector1.x * vector2.z - vector2.x * vector1.z),
			                     vector1.x * vector2.y - vector2.x * vector1.y);
		}
		
		public static float Distance(Vector3Custom vector1, Vector3Custom vector2)
		{
			float result;
			DistanceSquared(ref vector1, ref vector2, out result);
			return (float)Math.Sqrt(result);
		}
		
		public static void Distance(ref Vector3Custom value1, ref Vector3Custom value2, out float result)
		{
			DistanceSquared(ref value1, ref value2, out result);
			result = (float)Math.Sqrt(result);
		}
		
		public static float DistanceSquared(Vector3Custom value1, Vector3Custom value2)
		{
			float result;
			DistanceSquared(ref value1, ref value2, out result);
			return result;
		}
		
		public static void DistanceSquared(ref Vector3Custom value1, ref Vector3Custom value2, out float result)
		{
			result = (value1.x - value2.x) * (value1.x - value2.x) +
				(value1.y - value2.y) * (value1.y - value2.y) +
					(value1.z - value2.z) * (value1.z - value2.z);
		}
		
		public static Vector3Custom Divide(Vector3Custom value1, Vector3Custom value2)
		{
			value1.x /= value2.x;
			value1.y /= value2.y;
			value1.z /= value2.z;
			return value1;
		}
		
		public static Vector3Custom Divide(Vector3Custom value1, float value2)
		{
			float factor = 1 / value2;
			value1.x *= factor;
			value1.y *= factor;
			value1.z *= factor;
			return value1;
		}
		
		public static void Divide(ref Vector3Custom value1, float divisor, out Vector3Custom result)
		{
			float factor = 1 / divisor;
			result.x = value1.x * factor;
			result.y = value1.y * factor;
			result.z = value1.z * factor;
		}
		
		public static void Divide(ref Vector3Custom value1, ref Vector3Custom value2, out Vector3Custom result)
		{
			result.x = value1.x / value2.x;
			result.y = value1.y / value2.y;
			result.z = value1.z / value2.z;
		}
		
		public static float Dot(Vector3Custom vector1, Vector3Custom vector2)
		{
			return vector1.x * vector2.x + vector1.y * vector2.y + vector1.z * vector2.z;
		}
		
		public static void Dot(ref Vector3Custom vector1, ref Vector3Custom vector2, out float result)
		{
			result = vector1.x * vector2.x + vector1.y * vector2.y + vector1.z * vector2.z;
		}
		
		public override bool Equals(object obj)
		{
			return (obj is Vector3Custom) ? this == (Vector3Custom)obj : false;
		}
		
		public bool Equals(Vector3Custom other)
		{
			return this == other;
		}
		
		public override int GetHashCode()
		{
			return (int)(this.x + this.y + this.z);
		}

		
		public float Length()
		{
			float result;
			DistanceSquared(ref this, ref zero, out result);
			return (float)Math.Sqrt(result);
		}
		
		public float LengthSquared()
		{
			float result;
			DistanceSquared(ref this, ref zero, out result);
			return result;
		}
		
		public static Vector3Custom Multiply(Vector3Custom value1, Vector3Custom value2)
		{
			value1.x *= value2.x;
			value1.y *= value2.y;
			value1.z *= value2.z;
			return value1;
		}
		
		public static Vector3Custom Multiply(Vector3Custom value1, float scaleFactor)
		{
			value1.x *= scaleFactor;
			value1.y *= scaleFactor;
			value1.z *= scaleFactor;
			return value1;
		}
		
		public static void Multiply(ref Vector3Custom value1, float scaleFactor, out Vector3Custom result)
		{
			result.x = value1.x * scaleFactor;
			result.y = value1.y * scaleFactor;
			result.z = value1.z * scaleFactor;
		}
		
		public static void Multiply(ref Vector3Custom value1, ref Vector3Custom value2, out Vector3Custom result)
		{
			result.x = value1.x * value2.x;
			result.y = value1.y * value2.y;
			result.z = value1.z * value2.z;
		}
		
		public static Vector3Custom Negate(Vector3Custom value)
		{
			value = new Vector3Custom(-value.x, -value.y, -value.z);
			return value;
		}
		
		public static void Negate(ref Vector3Custom value, out Vector3Custom result)
		{
			result = new Vector3Custom(-value.x, -value.y, -value.z);
		}
		
		public void Normalize()
		{
			Normalize(ref this, out this);
		}
		
		public static Vector3Custom Normalize(Vector3Custom vector)
		{
			Normalize(ref vector, out vector);
			return vector;
		}
		
		public static void Normalize(ref Vector3Custom value, out Vector3Custom result)
		{
			float factor;
			Distance(ref value, ref zero, out factor);
			factor = 1f / factor;
			result.x = value.x * factor;
			result.y = value.y * factor;
			result.z = value.z * factor;
		}
		
		public static Vector3Custom Reflect(Vector3Custom vector, Vector3Custom normal)
		{
			// I is the original array
			// N is the normal of the incident plane
			// R = I - (2 * N * ( DotProduct[ I,N] ))
			Vector3Custom reflectedVector;
			// inline the dotProduct here instead of calling method
			float dotProduct = ((vector.x * normal.x) + (vector.y * normal.y)) + (vector.z * normal.z);
			reflectedVector.x = vector.x - (2.0f * normal.x) * dotProduct;
			reflectedVector.y = vector.y - (2.0f * normal.y) * dotProduct;
			reflectedVector.z = vector.z - (2.0f * normal.z) * dotProduct;
			
			return reflectedVector;
		}
		
		public static void Reflect(ref Vector3Custom vector, ref Vector3Custom normal, out Vector3Custom result)
		{
			// I is the original array
			// N is the normal of the incident plane
			// R = I - (2 * N * ( DotProduct[ I,N] ))
			
			// inline the dotProduct here instead of calling method
			float dotProduct = ((vector.x * normal.x) + (vector.y * normal.y)) + (vector.z * normal.z);
			result.x = vector.x - (2.0f * normal.x) * dotProduct;
			result.y = vector.y - (2.0f * normal.y) * dotProduct;
			result.z = vector.z - (2.0f * normal.z) * dotProduct;
			
		}
		

		
		public static Vector3Custom Subtract(Vector3Custom value1, Vector3Custom value2)
		{
			value1.x -= value2.x;
			value1.y -= value2.y;
			value1.z -= value2.z;
			return value1;
		}
		
		public static void Subtract(ref Vector3Custom value1, ref Vector3Custom value2, out Vector3Custom result)
		{
			result.x = value1.x - value2.x;
			result.y = value1.y - value2.y;
			result.z = value1.z - value2.z;
		}
		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(32);
			sb.Append("{X:");
			sb.Append(this.x);
			sb.Append(" Y:");
			sb.Append(this.y);
			sb.Append(" Z:");
			sb.Append(this.z);
			sb.Append("}");
			return sb.ToString();
		}
		

		
		/// <summary>
		/// Transforms a vector by a quaternion rotation.
		/// </summary>
		/// <param name="vec">The vector to transform.</param>
		/// <param name="quat">The quaternion to rotate the vector by.</param>
		/// <returns>The result of the operation.</returns>
		
		/// <summary>
		/// Transforms a vector by a quaternion rotation.
		/// </summary>
		/// <param name="vec">The vector to transform.</param>
		/// <param name="quat">The quaternion to rotate the vector by.</param>
		/// <param name="result">The result of the operation.</param>
		//        public static void Transform(ref Vector3 vec, ref Quaternion quat, out Vector3 result)
		//        {
		//        // Taken from the OpentTK implementation of Vector3
		//            // Since vec.W == 0, we can optimize quat * vec * quat^-1 as follows:
		//            // vec + 2.0 * cross(quat.xyz, cross(quat.xyz, vec) + quat.w * vec)
		//            Vector3 xyz = quat.Xyz, temp, temp2;
		//            Vector3.Cross(ref xyz, ref vec, out temp);
		//            Vector3.Multiply(ref vec, quat.W, out temp2);
		//            Vector3.Add(ref temp, ref temp2, out temp);
		//            Vector3.Cross(ref xyz, ref temp, out temp);
		//            Vector3.Multiply(ref temp, 2, out temp);
		//            Vector3.Add(ref vec, ref temp, out result);
		//        }
		
		/// <summary>
		/// Transforms a vector by a quaternion rotation.
		/// </summary>
		/// <param name="vec">The vector to transform.</param>
		/// <param name="quat">The quaternion to rotate the vector by.</param>
		/// <param name="result">The result of the operation.</param>
		

		
		//#endregion Public methods
		
		
		//#region Operators
		
		public static bool operator ==(Vector3Custom value1, Vector3Custom value2)
		{
			return value1.x == value2.x
				&& value1.y == value2.y
					&& value1.z == value2.z;
		}
		
		public static bool operator !=(Vector3Custom value1, Vector3Custom value2)
		{
			return !(value1 == value2);
		}
		
		public static Vector3Custom operator +(Vector3Custom value1, Vector3Custom value2)
		{
			value1.x += value2.x;
			value1.y += value2.y;
			value1.z += value2.z;
			return value1;
		}
		
		public static Vector3Custom operator -(Vector3Custom value)
		{
			value = new Vector3Custom(-value.x, -value.y, -value.z);
			return value;
		}
		
		public static Vector3Custom operator -(Vector3Custom value1, Vector3Custom value2)
		{
			value1.x -= value2.x;
			value1.y -= value2.y;
			value1.z -= value2.z;
			return value1;
		}
		
		public static Vector3Custom operator *(Vector3Custom value1, Vector3Custom value2)
		{
			value1.x *= value2.x;
			value1.y *= value2.y;
			value1.z *= value2.z;
			return value1;
		}
		
		public static Vector3Custom operator *(Vector3Custom value, float scaleFactor)
		{
			value.x *= scaleFactor;
			value.y *= scaleFactor;
			value.z *= scaleFactor;
			return value;
		}
		
		public static Vector3Custom operator *(float scaleFactor, Vector3Custom value)
		{
			value.x *= scaleFactor;
			value.y *= scaleFactor;
			value.z *= scaleFactor;
			return value;
		}
		
		public static Vector3Custom operator /(Vector3Custom value1, Vector3Custom value2)
		{
			value1.x /= value2.x;
			value1.y /= value2.y;
			value1.z /= value2.z;
			return value1;
		}
		
		public static Vector3Custom operator /(Vector3Custom value, float divider)
		{
			float factor = 1 / divider;
			value.x *= factor;
			value.y *= factor;
			value.z *= factor;
			return value;
		}
		
		//#endregion

		public Vector3Custom normalized {
			get	{
				return this*(1f/magnitude);

			}
			set{

			}
		}

		public float magnitude {
			get{
				return (Mathf.Sqrt(x*x + y*y + z*z));
			}
			set{

			}
		}

		public static implicit operator Vector3(Vector3Custom other)  // implicit digit to byte conversion operator
		{
			return new Vector3 (other.x, other.y, other.z);
		}

		public static implicit operator Vector2(Vector3Custom other)  // implicit digit to byte conversion operator
		{
			return new Vector2 (other.x, other.y);
		}

		public static implicit operator Vector3Custom(Vector3 other)  // implicit digit to byte conversion operator
		{
			return new Vector3Custom (other.x, other.y, other.z);
		}

		public Vector3Custom(Vector3Custom other){
			this.x = other.x;
			this.y = other.y;
			this.z = other.z;
		}

		public static float HorizontalDistanceSquared(Vector3Custom value1, Vector3Custom value2)
		{
			return (value1.x - value2.x) * (value1.x - value2.x)+
					(value1.z - value2.z) * (value1.z - value2.z);
		}


	}
}
