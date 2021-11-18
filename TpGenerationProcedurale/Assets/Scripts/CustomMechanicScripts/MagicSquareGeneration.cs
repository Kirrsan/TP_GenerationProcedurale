using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagicSquareGeneration : MonoBehaviour
{

    public int magicSquareSize = 3;

    public GameObject TextPrefab;

    private Text[] numbers;
    private int pointsToHave = 0;
    private void Start()
    {
        int numberOfText = magicSquareSize * magicSquareSize;
        numbers = new Text[numberOfText];
        for (int i = 0; i < numberOfText; i++)
        {
            numbers[i] = GameObject.Instantiate(TextPrefab, transform).GetComponent<Text>();
        }

        GetComponent<GridLayoutGroup>().constraintCount = magicSquareSize;

        GenerateSquare(magicSquareSize);

        pointsToHave = magicSquareSize * (magicSquareSize * magicSquareSize + 1) / 2;
    }

    private void GenerateSquare(int n)
    {
        int[,] magicSquare = new int[n, n];
        // Initialize position for 1
        int i = n / 2;
        int j = n - 1;
        // One by one put all values in magic square
        for (int num = 1; num <= n * n;)
        {
            if (i == -1 && j == n) // 3rd condition
            {
                j = n - 2;
                i = 0;
            }
            else
            {
                // 1st condition helper if next number
                // goes to out of square's right side
                if (j == n)
                    j = 0;

                // 1st condition helper if next number is
                // goes to out of square's upper side
                if (i < 0)
                    i = n - 1;
            }
            // 2nd condition
            if (magicSquare[i, j] != 0)
            {
                j -= 2;
                i++;
                continue;
            }
            else
                // set number
                magicSquare[i, j] = num++;

            // 1st condition
            j++;
            i--;
        }

        int index = 0;

        for (int k = 0; k < magicSquare.GetLength(0); k++)
        {
            for (int l = 0; l < magicSquare.GetLength(1); l++)
            {
                numbers[index].text = magicSquare[k, l].ToString();
                ++index;
            }
        }
    }

    public int GetPointsToHave()
    {
        return pointsToHave;
    }
}
