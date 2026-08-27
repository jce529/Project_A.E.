using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // 리스트를 사용하기 위해 추가

public class SlidingPuzzleManager : MonoBehaviour
{
    [Header("퍼즐의 가로줄 개수 (2x2면 2, 3x3이면 3 입력)")]
    public int gridSize = 3;

    [Header("퍼즐 조각(Image)들을 순서대로 넣으세요")]
    public Image[] puzzlePieces;

    [Header("정답 이미지들을 순서대로 넣으세요")]
    public Sprite[] correctSprites;

    [Header("투명한 빈칸용 이미지")]
    public Sprite emptySprite;

    private int emptyIndex;
    private bool isCleared = false;

    // 창이 켜질 때마다 실행됩니다.
    private void OnEnable()
    {
        if (!isCleared)
        {
            // 빈칸을 항상 맨 마지막 인덱스로 초기화
            emptyIndex = puzzlePieces.Length - 1;

            // 퍼즐 섞기 함수 호출!
            ShufflePuzzle();
        }
    }

    public void OnPieceClick(int clickedIndex)
    {
        if (isCleared) return;

        if (IsAdjacent(clickedIndex, emptyIndex))
        {
            SwapPieces(clickedIndex, emptyIndex);
            emptyIndex = clickedIndex;
            CheckClear();
        }
    }

    // 컴퓨터가 자동으로 퍼즐을 섞어주는 함수
    private void ShufflePuzzle()
    {
        // 2x2면 20번, 3x3이면 50번 무작위로 움직입니다.
        int shuffleCount = (gridSize == 2) ? 20 : 50;

        for (int i = 0; i < shuffleCount; i++)
        {
            List<int> validMoves = new List<int>();

            // 현재 빈칸(emptyIndex) 주변에 움직일 수 있는 조각들을 찾습니다.
            for (int j = 0; j < puzzlePieces.Length; j++)
            {
                if (IsAdjacent(j, emptyIndex))
                {
                    validMoves.Add(j);
                }
            }

            // 움직일 수 있는 조각 중 하나를 무작위로 골라서 스왑합니다.
            if (validMoves.Count > 0)
            {
                int randomPiece = validMoves[Random.Range(0, validMoves.Count)];
                SwapPieces(randomPiece, emptyIndex);
                emptyIndex = randomPiece;
            }
        }
    }

    // 조각의 이미지와 투명도를 교환하는 기능 (재사용하기 위해 따로 뺐습니다)
    private void SwapPieces(int index1, int index2)
    {
        Sprite tempSprite = puzzlePieces[index1].sprite;
        Color tempColor = puzzlePieces[index1].color;

        puzzlePieces[index1].sprite = puzzlePieces[index2].sprite;
        puzzlePieces[index1].color = puzzlePieces[index2].color;

        puzzlePieces[index2].sprite = tempSprite;
        puzzlePieces[index2].color = tempColor;
    }

    private bool IsAdjacent(int index1, int index2)
    {
        if (Mathf.Abs(index1 - index2) == 1 && (index1 / gridSize) == (index2 / gridSize)) return true;
        if (Mathf.Abs(index1 - index2) == gridSize) return true;
        return false;
    }

    private void CheckClear()
    {
        int totalPieces = puzzlePieces.Length;
        int checkCount = totalPieces - 1;

        for (int i = 0; i < checkCount; i++)
        {
            if (puzzlePieces[i].sprite != correctSprites[i]) return;
        }

        isCleared = true;

        puzzlePieces[totalPieces - 1].sprite = correctSprites[totalPieces - 1];
        puzzlePieces[totalPieces - 1].color = Color.white;

        OpengameManager.instance.isMap3Open = true;
        OpengameManager.instance.CheckMap5Condition();
    }
}