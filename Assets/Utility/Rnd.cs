using UnityEngine;

namespace Utility {
	public class Rnd {
		public int seed;

		public Rnd(int seed) {
			this.seed = seed;
		}

		public static int Next(int seed) {
			long a = ((long) seed) & 0xffffffffL;
			int n = (int) ((a * 279470273L) % 4294967291L) & 0x7fffffff;
			if (n == 0)
				n = 179375273;
			return n;
		}

		public static float Value(int seed) {
			return (seed % 279470273) / 279470272.0f;
		}

		public static float Range(int seed, float min, float max) {
			return min + (max - min) * Value(seed);
		}

		public static int Range(int seed, int min, int max) {
			return min + Mathf.Abs(seed) % (max - min + 1);
		}

		public float Value() {
			seed = Next(seed);
			return Value(seed);
		}

		public float Range(float min, float max) {
			seed = Next(seed);
			return Range(seed, min, max);
		}

		public int Range(int min, int max) {
			seed = Next(seed);
			return Range(seed, min, max);
		}
	}
}