using UnityEngine;

public class HandManager : MonoBehaviour
{
	public Transform handArea;
	public GameObject cardPrefab;
	public CardData[] startingCards;

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
	}
}

