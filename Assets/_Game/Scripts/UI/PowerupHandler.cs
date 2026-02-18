using UnityEngine;
using Bloomquartz.Puzzle;

namespace Bloomquartz.UI
{
    /// Thin bridge placed on the HUD canvas so Unity's persistent button
    /// listeners can target a scene-resident object instead of the Board
    /// directly (which may not exist when the Editor setup script runs).
    public class PowerupHandler : MonoBehaviour
    {
        public void BuyMoves()      => Board.Instance?.BuyMoves();
        public void BombPowerUp()   => Board.Instance?.BombPowerUp();
        public void ShufflePowerUp()=> Board.Instance?.ShufflePowerUp();
    }
}
