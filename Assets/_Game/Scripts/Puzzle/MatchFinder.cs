using System.Collections.Generic;
using Bloomquartz.Gems;

namespace Bloomquartz.Puzzle
{
    public static class MatchFinder
    {
        public static List<Tile> FindMatches(Tile[,] grid, int width, int height)
        {
            HashSet<Tile> matched = new HashSet<Tile>();

            // Horizontal
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width - 2; x++)
                {
                    if (grid[x, y].IsEmpty()) continue;
                    GemType t = grid[x, y].GemType;
                    if (grid[x + 1, y].GemType == t && grid[x + 2, y].GemType == t &&
                        !grid[x + 1, y].IsEmpty() && !grid[x + 2, y].IsEmpty())
                    {
                        matched.Add(grid[x, y]);
                        matched.Add(grid[x + 1, y]);
                        matched.Add(grid[x + 2, y]);

                        // extend match
                        int ex = x + 3;
                        while (ex < width && !grid[ex, y].IsEmpty() && grid[ex, y].GemType == t)
                            matched.Add(grid[ex++, y]);
                    }
                }
            }

            // Vertical
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height - 2; y++)
                {
                    if (grid[x, y].IsEmpty()) continue;
                    GemType t = grid[x, y].GemType;
                    if (grid[x, y + 1].GemType == t && grid[x, y + 2].GemType == t &&
                        !grid[x, y + 1].IsEmpty() && !grid[x, y + 2].IsEmpty())
                    {
                        matched.Add(grid[x, y]);
                        matched.Add(grid[x, y + 1]);
                        matched.Add(grid[x, y + 2]);

                        int ey = y + 3;
                        while (ey < height && !grid[x, ey].IsEmpty() && grid[x, ey].GemType == t)
                            matched.Add(grid[x, ey++]);
                    }
                }
            }

            return new List<Tile>(matched);
        }
    }
}
