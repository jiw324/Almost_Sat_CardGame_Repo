using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
	[Header("UI References")]
	public Image artworkImage;
	public TMP_Text titleText;
	public TMP_Text descriptionText;
	public TMP_Text costText;

	private CardData cardData;

	public void Initialize(CardData card)
	{
		cardData = card;

		titleText.text = cardData.cardName;
		descriptionText.text = cardData.description;
		//costText.text = cardData.cost.ToString();
		artworkImage.sprite = card.artwork;
	}

	public CardData GetCardData() => cardData;
}

