﻿using System;
using System.Linq;
using System.Threading.Tasks;

namespace B58CheckAsync
{
    internal class ArrayHelpers
    {
        public static Task<T[]> ConcatArrays<T>(params T[][] arrays)
        {
            var result = new T[arrays.Sum(arr => arr.Length)];
            var offset = 0;

            foreach (var arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                offset += arr.Length;
            }

            return Task.FromResult(result);
        }

        public static Task<T[]> ConcatArrays<T>(T[] arr1, T[] arr2)
        {
            var result = new T[arr1.Length + arr2.Length];
            Buffer.BlockCopy(arr1, 0, result, 0, arr1.Length);
            Buffer.BlockCopy(arr2, 0, result, arr1.Length, arr2.Length);

            return Task.FromResult(result);
        }

        public static Task<T[]> SubArray<T>(T[] arr, int start, int length)
        {
            var result = new T[length];
            Buffer.BlockCopy(arr, start, result, 0, length);

            return Task.FromResult(result);
        }

        public static async Task<T[]> SubArray<T>(T[] arr, int start)
        {
            return await SubArray(arr, start, arr.Length - start);
        }
    }
}