using System;

namespace XamarinAndroidCleverPuzzle.Solver
{
    public static class PuzzleMixer	
    {
        public static void Shuffle<T>(Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static int[] GetPuzzleArray(int numOfTiles, int dimension, int numberOfChanges, bool keepZeroInCorner, Random random)
        {
            int[] tiles = new int[numOfTiles];
            for (int i = numOfTiles - 2; i >= 0; --i)
            {
                tiles[i] = (i + 1);
            }
            tiles[numOfTiles - 1] = 0;
            int maxTilesToSwap;
            if (keepZeroInCorner)
                maxTilesToSwap = numOfTiles - 1;
            else
                maxTilesToSwap = numOfTiles;

            for (int i = numberOfChanges; i >= 0; --i)
            {
                int rand1 = random.Next(maxTilesToSwap);
                int rand2 = random.Next(maxTilesToSwap);
                if (rand1 == rand2)
                {
                    if (rand1 < (maxTilesToSwap << 1))
                        rand2 = random.Next(maxTilesToSwap - rand1) + rand1;
                    else
                        rand2 = random.Next(rand1);
                }
                swap(rand1, rand2, tiles);
            }
            if (!isValidPermutation(tiles, dimension))
            {
                if (tiles[0] != 0 && tiles[1] != 0)
                    swap(0, 1, tiles);
                else
                    swap(2, 3, tiles);
            }
            return tiles;
        }

        private static bool isValidPermutation(int[] state, int dim)
        {
            int inversions = 0;
            for (int i = 0; i < state.Length; ++i)
            {
                int iTile = state[i];
                if (iTile != 0)
                {
                    for (int j = i + 1; j < state.Length; ++j)
                    {
                        int jTile = state[j];
                        if (jTile != 0 && jTile < iTile)
                        {
                            ++inversions;
                        }
                    }
                }
                else
                {
                    if ((dim & 0x1) == 0)
                    {
                        inversions += (1 + i / dim);
                    }
                }
            }
            if ((inversions & 0x1) == 1) return false;
            return true;
        }

        private static void swap(int i, int j, int[] A)
        {
            int temp = A[i];
            A[i] = A[j];
            A[j] = temp;
        }
    }
    }
