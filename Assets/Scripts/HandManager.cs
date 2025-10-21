using UnityEngine;

public class HandManager : MonoBehaviour
{
	public Transform handArea;
	public GameObject cardPrefab;
	public CardData[] startingCards;
    public EntityBase defaultTarget;

    private void Start()
	{
		foreach (var card in startingCards)
		{
			SpawnCard(card);
		}
	}

	public void SpawnCard(CardData cardData)
	{
		GameObject cardObj = Instantiate(cardPrefab, handArea);
		CardUI cardUI = cardObj.GetComponent<CardUI>();
		cardUI.Initialize(cardData);
        var pc = cardObj.GetComponent<PlayableCard>();
        if (pc != null) pc.demoTarget = defaultTarget;   // inject target here
    }
}

