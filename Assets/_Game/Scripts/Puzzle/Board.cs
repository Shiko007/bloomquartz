using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bloomquartz.Gems;
using Bloomquartz.Juice;
using Bloomquartz.UI;
using Bloomquartz.Core;

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

            // Ensure SaveSystem exists when starting directly from PuzzleBoard scene
            if (SaveSystem.Instance == null)
                new GameObject("SaveSystem").AddComponent<SaveSystem>();
        }

        private void Start()
        {
            // Fit the camera to the board dimensions on this device's aspect ratio,
            // then shift the board down so it sits below the HUD strip.
            FitCameraToBoard();

            // Scale moves from save level (falls back to Inspector value if no save)
            int level = SaveSystem.Instance?.Data.highestLevel ?? 0;
            startMoves = LevelConfig.GetMoves(level);

            // Evolution bonus: sum all plant evolution levels → +10% score per level
            int totalEvo = 0;
            var plants = SaveSystem.Instance?.Data.plants;
            if (plants != null)
                foreach (var p in plants)
                    totalEvo += p.evolutionLevel;

            ScoreManager.Instance.SetEvolutionBonus(totalEvo);
            ScoreManager.Instance.Init(startMoves);
            HUDController.Instance?.RefreshGemCount();
            GenerateBoard();

            // Self-bootstrap juice components if scene setup hasn't been re-run
            if (FloatingTextPool.Instance == null)
                new GameObject("FloatingTextPool").AddComponent<FloatingTextPool>();
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

            // Deadlock detection: if game isn't over but no valid swap exists, force loss
            if (WinLoseController.Instance != null && !WinLoseController.Instance.IsGameOver)
            {
                if (!HasValidMove())
                    WinLoseController.Instance.TriggerNoMoves();
            }
        }

        private IEnumerator SwapTiles(Tile a, Tile b)
        {
            GemType tempType = a.GemType;
            a.SetGemType(b.GemType, animate: true);
            b.SetGemType(tempType, animate: true);
            Audio.AudioManager.Instance?.PlaySFX("swap");
            yield return new WaitForSeconds(0.2f);
        }

        private IEnumerator ProcessMatches(List<Tile> matched)
        {
            ScoreManager.Instance.RegisterMatch(matched.Count, _isCascade);

            // Camera shake — heavier on cascades
            float shakeDur = _isCascade ? 0.22f : 0.14f;
            float shakeMag = _isCascade ? 0.10f : 0.05f;
            CameraShaker.Instance?.Shake(shakeDur, shakeMag);

            // Floating score text at centroid of matched tiles
            Vector3 centroid = Vector3.zero;
            foreach (Tile t in matched) centroid += t.transform.position;
            centroid /= matched.Count;
            Color popColor = _isCascade ? new Color(1f, 0.9f, 0.2f) : Color.white;
            FloatingTextPool.Instance?.Spawn(centroid, "+" + matched.Count, popColor);

            _isCascade = true;

            Audio.AudioManager.Instance?.PlaySFX("gemPop");

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

        private void FitCameraToBoard()
        {
            var cam = Camera.main;
            if (cam == null || !cam.orthographic) return;

            float boardW = width  * tileSize;
            float boardH = height * tileSize;
            float aspect = (float)Screen.width / Screen.height;

            // Minimum ortho size to fully show the board, then add 5% breathing room
            float sizeForWidth  = (boardW / 2f) / aspect;
            float sizeForHeight =  boardH / 2f;
            cam.orthographicSize = Mathf.Max(sizeForWidth, sizeForHeight) * 1.05f;

            // Place the board so its TOP sits just below the HUD strip.
            // The HUD (safe area + 4 UI rows) occupies roughly the top 24% of screen height.
            // board_top  = cam_top - 24% of visible_height
            // board_center = board_top - boardH/2
            float boardY = cam.orthographicSize * (1f - 2f * 0.24f) - boardH / 2f;
            transform.position = new Vector3(0f, boardY, 0f);
        }

        // ── Deadlock detection ────────────────────────────────────────────────

        /// Returns true if at least one adjacent swap would create a 3+ match.
        private bool HasValidMove()
        {
            GemType[,] snap = SnapshotBoard();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Try horizontal swap with right neighbour
                    if (x + 1 < width)
                    {
                        SwapInSnapshot(snap, x, y, x + 1, y);
                        if (SnapshotHasMatch(snap)) return true;
                        SwapInSnapshot(snap, x, y, x + 1, y); // undo
                    }
                    // Try vertical swap with upper neighbour
                    if (y + 1 < height)
                    {
                        SwapInSnapshot(snap, x, y, x, y + 1);
                        if (SnapshotHasMatch(snap)) return true;
                        SwapInSnapshot(snap, x, y, x, y + 1); // undo
                    }
                }
            }
            return false;
        }

        private GemType[,] SnapshotBoard()
        {
            var snap = new GemType[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    snap[x, y] = _grid[x, y].GemType;
            return snap;
        }

        private static void SwapInSnapshot(GemType[,] s, int x1, int y1, int x2, int y2)
        {
            GemType tmp = s[x1, y1];
            s[x1, y1]   = s[x2, y2];
            s[x2, y2]   = tmp;
        }

        // ── Power-ups ─────────────────────────────────────────────────────────

        /// Spend 200 gems to buy 5 extra moves.
        public void BuyMoves()
        {
            const int cost = 200;
            if (SaveSystem.Instance == null) return;
            if (SaveSystem.Instance.Data.totalGems < cost)
            {
                HUDController.Instance?.ShowNotEnoughGems(cost);
                return;
            }
            SaveSystem.Instance.Data.totalGems -= cost;
            SaveSystem.Instance.Save();
            ScoreManager.Instance.AddMoves(5);
            HUDController.Instance?.RefreshGemCount();
            Audio.AudioManager.Instance?.PlaySFX("uiTap");
            HapticFeedback.Light();
        }

        /// Spend 250 gems to clear all tiles of the most common colour.
        public void BombPowerUp()
        {
            if (_isProcessing) return;
            const int cost = 250;
            if (SaveSystem.Instance == null) return;
            if (SaveSystem.Instance.Data.totalGems < cost)
            {
                HUDController.Instance?.ShowNotEnoughGems(cost);
                return;
            }
            SaveSystem.Instance.Data.totalGems -= cost;
            SaveSystem.Instance.Save();
            HUDController.Instance?.RefreshGemCount();
            StartCoroutine(DoBomb());
        }

        private IEnumerator DoBomb()
        {
            _isProcessing = true;
            _isCascade    = false;

            // Find the most common non-empty gem type
            var counts = new Dictionary<GemType, int>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (!_grid[x, y].IsEmpty())
                    {
                        var t = _grid[x, y].GemType;
                        counts[t] = counts.TryGetValue(t, out var c) ? c + 1 : 1;
                    }

            GemType target = GemType.Ruby;
            int maxCount = 0;
            foreach (var kv in counts)
                if (kv.Value > maxCount) { maxCount = kv.Value; target = kv.Key; }

            var toRemove = new List<Tile>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (!_grid[x, y].IsEmpty() && _grid[x, y].GemType == target)
                        toRemove.Add(_grid[x, y]);

            if (toRemove.Count > 0)
                yield return StartCoroutine(ProcessMatches(toRemove));

            _isProcessing = false;
            WinLoseController.Instance?.CheckEndCondition();
        }

        /// Spend 150 gems to randomly shuffle all tiles on the board.
        public void ShufflePowerUp()
        {
            if (_isProcessing) return;
            const int cost = 150;
            if (SaveSystem.Instance == null) return;
            if (SaveSystem.Instance.Data.totalGems < cost)
            {
                HUDController.Instance?.ShowNotEnoughGems(cost);
                return;
            }
            SaveSystem.Instance.Data.totalGems -= cost;
            SaveSystem.Instance.Save();
            HUDController.Instance?.RefreshGemCount();
            StartCoroutine(DoShuffle());
        }

        private IEnumerator DoShuffle()
        {
            _isProcessing = true;
            CameraShaker.Instance?.Shake(0.3f, 0.08f);
            Audio.AudioManager.Instance?.PlaySFX("swap");

            // Collect all current gem types, Fisher-Yates shuffle, reassign
            var types = new List<GemType>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (!_grid[x, y].IsEmpty())
                        types.Add(_grid[x, y].GemType);

            for (int i = types.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (types[i], types[j]) = (types[j], types[i]);
            }

            int idx = 0;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (!_grid[x, y].IsEmpty())
                        _grid[x, y].SetGemType(types[idx++], animate: true);

            yield return new WaitForSeconds(0.3f);

            // Clear any immediate matches the shuffle created
            _isCascade = false;
            List<Tile> cascade = MatchFinder.FindMatches(_grid, width, height);
            if (cascade.Count > 0)
                yield return StartCoroutine(ProcessMatches(cascade));

            _isProcessing = false;
            WinLoseController.Instance?.CheckEndCondition();
        }

        /// Checks the snapshot for any horizontal or vertical run of 3+.
        private bool SnapshotHasMatch(GemType[,] s)
        {
            for (int y = 0; y < height; y++)
            {
                int run = 1;
                for (int x = 1; x < width; x++)
                {
                    if (!_grid[x, y].IsEmpty() && s[x, y] == s[x - 1, y]) { if (++run >= 3) return true; }
                    else run = 1;
                }
            }
            for (int x = 0; x < width; x++)
            {
                int run = 1;
                for (int y = 1; y < height; y++)
                {
                    if (!_grid[x, y].IsEmpty() && s[x, y] == s[x, y - 1]) { if (++run >= 3) return true; }
                    else run = 1;
                }
            }
            return false;
        }
    }
}
