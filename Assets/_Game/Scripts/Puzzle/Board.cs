using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bloomquartz.Gems;
using Bloomquartz.Juice;
using Bloomquartz.UI;

namespace Bloomquartz.Puzzle
{
    public class Board : MonoBehaviour
    {
        public static Board Instance { get; private set; }

        [Header("Board Config")]
        [SerializeField] private int width    = 7;
        [SerializeField] private int height   = 7;
        [SerializeField] private float tileSize = 1.1f;
        [SerializeField] private int startMoves = 30;

        [Header("Prefabs")]
        [SerializeField] private Tile tilePrefab;

        private Tile[,] _grid;
        private Tile _selectedTile;
        private bool _isProcessing;
        private bool _isCascade;

        public bool IsProcessing => _isProcessing;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            ScoreManager.Instance.Init(startMoves);
            GenerateBoard();
        }

        private void GenerateBoard()
        {
            _grid = new Tile[width, height];
            Vector3 origin = transform.position - new Vector3(width * tileSize / 2f, height * tileSize / 2f, 0);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 pos = origin + new Vector3(x * tileSize, y * tileSize, 0);
                    Tile tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                    tile.Init(x, y, GetSafeGemType(x, y));
                    _grid[x, y] = tile;
                }
            }
        }

        /// Returns a random GemType that won't create a 3-in-a-row/column
        /// with already-placed neighbours to the left and below.
        private GemType GetSafeGemType(int x, int y)
        {
            var all = (GemType[])System.Enum.GetValues(typeof(GemType));
            var forbidden = new System.Collections.Generic.HashSet<GemType>();

            // Horizontal: block if left two tiles share the same type
            if (x >= 2 &&
                _grid[x - 1, y].GemType == _grid[x - 2, y].GemType)
                forbidden.Add(_grid[x - 1, y].GemType);

            // Vertical: block if bottom two tiles share the same type
            if (y >= 2 &&
                _grid[x, y - 1].GemType == _grid[x, y - 2].GemType)
                forbidden.Add(_grid[x, y - 1].GemType);

            // Build allowed list and pick randomly
            var allowed = new System.Collections.Generic.List<GemType>();
            foreach (var g in all)
                if (!forbidden.Contains(g))
                    allowed.Add(g);

            // Fallback to full list if somehow all are forbidden
            if (allowed.Count == 0)
                return all[Random.Range(0, all.Length)];

            return allowed[Random.Range(0, allowed.Count)];
        }

        public void OnTileSelected(Tile tile)
        {
            if (_isProcessing) return;
            if (ScoreManager.Instance.MovesLeft <= 0) return;
            if (WinLoseController.Instance != null && WinLoseController.Instance.IsGameOver) return;

            if (_selectedTile == null)
            {
                _selectedTile = tile;
                tile.SetSelected(true);
            }
            else if (_selectedTile == tile)
            {
                _selectedTile.SetSelected(false);
                _selectedTile = null;
            }
            else if (AreAdjacent(_selectedTile, tile))
            {
                StartCoroutine(SwapAndMatch(_selectedTile, tile));
                _selectedTile.SetSelected(false);
                _selectedTile = null;
            }
            else
            {
                _selectedTile.SetSelected(false);
                _selectedTile = tile;
                tile.SetSelected(true);
            }
        }

        private IEnumerator SwapAndMatch(Tile a, Tile b)
        {
            _isProcessing = true;
            _isCascade    = false;

            yield return StartCoroutine(SwapTiles(a, b));

            List<Tile> matched = MatchFinder.FindMatches(_grid, width, height);

            if (matched.Count == 0)
            {
                yield return StartCoroutine(SwapTiles(a, b));
                _isProcessing = false;
                yield break;
            }

            // Valid move — consume one move
            ScoreManager.Instance.UseMove();

            yield return StartCoroutine(ProcessMatches(matched));

            _isProcessing = false;

            WinLoseController.Instance?.CheckEndCondition();
        }

        private IEnumerator SwapTiles(Tile a, Tile b)
        {
            GemType tempType = a.GemType;
            a.SetGemType(b.GemType, animate: true);
            b.SetGemType(tempType, animate: true);
            yield return new WaitForSeconds(0.2f);
        }

        private IEnumerator ProcessMatches(List<Tile> matched)
        {
            ScoreManager.Instance.RegisterMatch(matched.Count, _isCascade);
            _isCascade = true;

            foreach (Tile t in matched)
            {
                JuiceManager.Instance.PlayGemPop(t.transform.position, t.GemType);
                HapticFeedback.Light();
                t.ClearTile();
            }

            yield return new WaitForSeconds(0.15f);
            yield return StartCoroutine(CollapseColumns());
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(FillBoard());

            List<Tile> cascadeMatches = MatchFinder.FindMatches(_grid, width, height);
            if (cascadeMatches.Count > 0)
                yield return StartCoroutine(ProcessMatches(cascadeMatches));
        }

        private IEnumerator CollapseColumns()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    if (_grid[x, y].IsEmpty())
                    {
                        for (int above = y + 1; above < height; above++)
                        {
                            if (!_grid[x, above].IsEmpty())
                            {
                                _grid[x, y].SetGemType(_grid[x, above].GemType, animate: true);
                                _grid[x, above].ClearTile();
                                break;
                            }
                        }
                    }
                }
            }
            yield return new WaitForSeconds(0.2f);
        }

        private IEnumerator FillBoard()
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (_grid[x, y].IsEmpty())
                        _grid[x, y].SetGemType(GetRandomGemType(), animate: true);

            yield return new WaitForSeconds(0.15f);
        }

        private bool AreAdjacent(Tile a, Tile b)
        {
            int dx = Mathf.Abs(a.GridX - b.GridX);
            int dy = Mathf.Abs(a.GridY - b.GridY);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        private GemType GetRandomGemType()
        {
            int count = System.Enum.GetValues(typeof(GemType)).Length;
            return (GemType)Random.Range(0, count);
        }

        public Tile GetTile(int x, int y) => _grid[x, y];
    }
}
