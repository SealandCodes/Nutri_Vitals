using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject itemTextObj;
    [SerializeField] private TextMeshProUGUI itemText;

    private CharacterController characterController;
    private int desiredLane = 1;
    private Vector2 endTouchPosition;
    private Vector2 startTouchPosition;
    private Vector3 direction;
    public float laneDistance = 2.5f;
    public int smoothMovementSpeed = 30;
    public static float forwardSpeed = 150;
    public static float swipeSensitivity;
    public static Vector3 targetPosition;

    [SerializeField] private GameObject[] modelCharacterTop;
    [SerializeField] private GameObject[] modelCharacterOuterTop;
    [SerializeField] private GameObject[] modelCharacterBottom;
    [SerializeField] private GameObject[] modelCharacterHair;
    [SerializeField] private GameObject[] modelCharacterOufits;

    private bool rewardIsEquippedShoes;
    private bool rewardIsEquippedCap;
    private bool rewardIsEquippedBag;
    private bool rewardIsEquippedOutfit;

    private int characterIndex;
    Vector3 returnPos;

    private int outfitMaleOrFemale;

    void Start()
    {
        rewardIsEquippedShoes = PlayerPrefs.GetInt("_rewardIsEquippedShoes", 0) == 1;
        rewardIsEquippedCap = PlayerPrefs.GetInt("_rewardIsEquippedCap", 0) == 1;
        rewardIsEquippedBag = PlayerPrefs.GetInt("_rewardIsEquippedBag", 0) == 1;
        rewardIsEquippedOutfit = PlayerPrefs.GetInt("_rewardIsEquippedOutfit", 0) == 1;

        //CHARACTER
        characterIndex = PlayerPrefs.GetInt("_characterIndex", 0);

        outfitMaleOrFemale =
            characterIndex == 0 || characterIndex == 3
            ? 0
            : 1;

        //HAIR
        modelCharacterHair[characterIndex].SetActive(true);

        //TOPS AND BOTTOMS
        if (rewardIsEquippedOutfit)
        {
            modelCharacterOufits[3].SetActive(true);
            modelCharacterOufits[4].SetActive(true);
        }
        else
        {
                modelCharacterTop[outfitMaleOrFemale].SetActive(true);
                modelCharacterOuterTop[outfitMaleOrFemale].SetActive(true);
                modelCharacterBottom[outfitMaleOrFemale].SetActive(true);
        }

        //SHOES
        modelCharacterOufits[0].SetActive(rewardIsEquippedShoes);
        modelCharacterOufits[5].SetActive(!rewardIsEquippedShoes);

        //CAP
        modelCharacterOufits[1].SetActive(rewardIsEquippedCap);

        //BAG
        modelCharacterOufits[2].SetActive(rewardIsEquippedBag);



        characterController = GetComponent<CharacterController>();

        Vector3 returnPos = itemTextObj.transform.position;
    }

    void Update()
    {

        if (StateManager.IsMoving)
        {

            direction.z = forwardSpeed;

            #region SWIPE LEFT AND RIGHT CODES

            if (Input.touchCount > 0 
                && Input.GetTouch(0).phase == TouchPhase.Began)

                startTouchPosition = Input.GetTouch(0).position;

            if (Input.touchCount > 0 
                && Input.GetTouch(0).phase == TouchPhase.Ended)
            {

                endTouchPosition = Input.GetTouch(0).position;

                if (StateManager.EnergyState == StateManager.ENERGY.LOW_DANGER 
                    || StateManager.EnergyState == StateManager.ENERGY.HIGH_DANGER)
                {

                    #region WITH SWIPE SENSITIVITY

                    float swipeDistance = endTouchPosition.x - startTouchPosition.x;

                    if (swipeDistance > swipeSensitivity)

                        // Right Swipe
                        OnRightSwipe();

                    else if (swipeDistance < -swipeSensitivity)

                        // Left Swipe
                        OnLeftSwipe();

                    #endregion

                }

                else if (StateManager.EnergyState == StateManager.ENERGY.DRUNK)//FOR FUTURE UPDATE
                {


                    #region WITH REVERSE SWIPE SENSITIVITY

                    if (endTouchPosition.x > startTouchPosition.x)

                        //LeftSwipe
                        OnLeftSwipe();

                    else if (endTouchPosition.x < startTouchPosition.x)

                        //RightSwipe
                        OnRightSwipe();

                    #endregion

                }

                else
                {

                    #region NO SWIPE SENSITIVITY

                    if (endTouchPosition.x < startTouchPosition.x)

                        //LeftSwipe
                        OnLeftSwipe();

                    else if (endTouchPosition.x > startTouchPosition.x)

                        //RightSwipe
                        OnRightSwipe();

                    #endregion

                }

            }
            #endregion

            #region PLAYER MOVEMENT CALCULATION

            targetPosition =
                transform.position.z * transform.forward +
                transform.position.y * transform.up;

            if (desiredLane == 2)

                targetPosition += Vector3.right * laneDistance;

            else if (desiredLane == 0)

                targetPosition += Vector3.left * laneDistance;

            characterController.Move(direction * Time.deltaTime);
            transform.position = Vector3.Lerp(
                transform.position, 
                targetPosition, 
                smoothMovementSpeed * Time.deltaTime);
            characterController.center = characterController.center;

            #endregion

        }

        //BLOCKS ENERGY DECREMENT
        if (StateManager.PowerUpTypeState == StateManager.POWER_UP_TYPE.GO)
        {

            HUDManager.ResetEnergyPoints();
            return;

        }
        else if (!StateManager.IsMoving)
        {
            return;

        }

        //DECREASE ENERGY OVER TIME
        HUDManager.DecreaseEnergyOverTime();

    }

    #region FOOD COLLIDER

    private void OnControllerColliderHit(ControllerColliderHit _controllerColliderHit)
    {

        string hit = _controllerColliderHit.transform.tag;
        StateManager.HitState = StateManager.GetHit(hit);

        if (StateManager.HitState != StateManager.HIT.JUNK)
        {

            if (StateManager.HitState == StateManager.HIT.GO)
            {
                FindObjectOfType<SoundManager>().PlayGo();
                StartCoroutine(ShowCollected("+1\nGo", returnPos));
            }
            else if (StateManager.HitState == StateManager.HIT.GROW)
            {
                FindObjectOfType<SoundManager>().PlayGrow();
                StartCoroutine(ShowCollected("+1\nGrow", returnPos));
            }
            else if (StateManager.HitState == StateManager.HIT.GLOW)
            {
                FindObjectOfType<SoundManager>().PlayGlow();
                StartCoroutine(ShowCollected("+1\nGlow", returnPos));
            }

        }

        else
        {
            if (StateManager.HitState == StateManager.HIT.JUNK)
            {
                FindObjectOfType<SoundManager>().PlayOhno();
                StartCoroutine(ShowCollected("Junk Food\nOh No!", returnPos));
            }
        }
            

        #region WITH POWER UP COLLIDER

        if (StateManager.PowerUpState == StateManager.POWER_UP.POWER_UP)
        {

            if (StateManager.PowerUpTypeState == StateManager.POWER_UP_TYPE.GO)
            {

                if (StateManager.HitState == StateManager.HIT.GO)

                    HUDManager.UpdateScoreEnergyPoints(25, 0f);

            }
            else if (StateManager.PowerUpTypeState == StateManager.POWER_UP_TYPE.GROW)
            {

                if (StateManager.HitState == StateManager.HIT.GROW)

                    FindObjectOfType<HUDManager>().healthBarFill.fillAmount += 0.1667f;

                else if (StateManager.HitState != StateManager.HIT.JUNK)
                    
                    HUDManager.GoodFoodBenefits(1);

                else
                {

                    HUDManager.ResetFoodPoints();
                    HUDManager.UpdateScoreEnergyPoints(-100, 0.75f);

                }

            }
            else if (StateManager.PowerUpTypeState == StateManager.POWER_UP_TYPE.GLOW)
            {

                if (StateManager.HitState == StateManager.HIT.GLOW)

                    HUDManager.GoodFoodBenefits(3);

                else if (StateManager.HitState != StateManager.HIT.JUNK)

                    HUDManager.GoodFoodBenefits(2);

                else
                {

                    HUDManager.ResetFoodPoints();
                    HUDManager.UpdateScoreEnergyPoints(-100, 0.75f);

                }

            }
            else
            {

                if (StateManager.HitState != StateManager.HIT.JUNK)

                    HUDManager.GoodFoodBenefits(1);

                else
                {

                    HUDManager.ResetFoodPoints();
                    HUDManager.UpdateScoreEnergyPoints(-100, 0.75f);

                }

            }

        }

        #endregion

        #region NO POWER UP COLLIDER

        else
        {

            if (StateManager.HitState != StateManager.HIT.JUNK)

                HUDManager.GoodFoodBenefits(1);

            if (StateManager.HitState == StateManager.HIT.GO)

                HUDManager.UpdateFoodPoints(1, 0, 0);

            else if (StateManager.HitState == StateManager.HIT.GROW)

                HUDManager.UpdateFoodPoints(0, 1, 0);

            else if (StateManager.HitState == StateManager.HIT.GLOW)

                HUDManager.UpdateFoodPoints(0, 0, 1);

            else if (StateManager.HitState == StateManager.HIT.JUNK)
            {

                HUDManager.ResetFoodPoints();
                HUDManager.UpdateScoreEnergyPoints(-100, 0.75f);

            }

        }

        #endregion

        Destroy(_controllerColliderHit.gameObject);

    }

    #endregion

    private void OnRightSwipe()
    {

        desiredLane++;

        if (desiredLane == 3)
            
            desiredLane = 2;

    }

    private void OnLeftSwipe()
    {

        desiredLane--;

        if (desiredLane == -1)

            desiredLane = 0;

    }




    private IEnumerator ShowCollected(string collectedIndicator, Vector3 xreturnPos)
    {

        itemText.text = collectedIndicator;
        yield return new WaitForSeconds(0.5f);
        itemText.text = "";

    }

}
