using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderBoardItem : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI positionText;
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private TextMeshProUGUI scoreText;

    public void SetValues(string position, string name, string score){
        positionText.text = position;
        nameText.text = name;
        scoreText.text = score;
    }
}
