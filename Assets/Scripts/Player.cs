using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;

    private Vector2 moveDirection;

    private void Awake()
    {
        moveDirection = Vector2.up;
        StartCoroutine(ScaleUp());
    }

    private void OnEnable()
    {
        GameplayManager.Instance.GameEnd += GameEnded;
    }

    private void OnDisable()
    {
        GameplayManager.Instance.GameEnd -= GameEnded;
    }

    [SerializeField] private GameObject _clickParticle, _scoreParticle, _playerParticle;
    [SerializeField] private AudioClip _moveClip, _scoreSoundClip;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            SoundManager.Instance.PlaySound(_moveClip);

            Vector3 mousPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousPos2D = new Vector2(mousPos.x, mousPos.y);

            moveDirection = (mousPos2D - (Vector2)transform.position).normalized;

            Destroy(Instantiate(_clickParticle, new Vector3(mousPos.x, mousPos.y, 0f), Quaternion.identity),1f);
        }

        float cosAngle = Mathf.Acos(moveDirection.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, cosAngle * (moveDirection.y < 0f ? -1f : 1f));
    }

    private void FixedUpdate()
    {
        transform.position += (Vector3)(_moveSpeed * Time.fixedDeltaTime * moveDirection);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Side"))
        {
            moveDirection.x *= -1f;
        }

        if (collision.CompareTag("Top"))
        {
            moveDirection.y *= -1f;
        }

        if(collision.CompareTag("Score"))
        {

            SoundManager.Instance.PlaySound(_scoreSoundClip);
            Destroy(Instantiate(_scoreParticle, collision.gameObject.transform.position, Quaternion.identity), 1f);
            GameplayManager.Instance.UpdateScore();
            StartCoroutine(ScoreDestroy(collision.gameObject));
        }
    }

    [SerializeField] private AnimationClip _scoreDestroyClip;

    private IEnumerator ScoreDestroy(GameObject scoreObject)
    {
        scoreObject.GetComponent<Collider2D>().enabled = false;
        scoreObject.GetComponent<Animator>().Play(_scoreDestroyClip.name, -1, 0f);
        yield return new WaitForSeconds(_scoreDestroyClip.length);
        Destroy(scoreObject);
    }

    [SerializeField] private float _animationTime;
    [SerializeField] private AnimationCurve _scaleUpCurve, _scaleDownCurve;

    private IEnumerator ScaleUp()
    {
        float timeElapsed = 0f;
        float speed = 1 / _animationTime;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        Vector3 scaleOffset = endScale - startScale;

        while(timeElapsed < 1f)
        {
            timeElapsed += speed * Time.deltaTime;
            transform.localScale = startScale + _scaleUpCurve.Evaluate(timeElapsed) * scaleOffset;
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    private void GameEnded()
    {
        StartCoroutine(ScaleDown());
    }

    private IEnumerator ScaleDown()
    {
        Destroy(Instantiate(_playerParticle, transform.position, Quaternion.identity), 1f);

        float timeElapsed = 0f;
        float speed = 1 / _animationTime;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.zero;
        Vector3 scaleOffset = endScale - startScale;

        while (timeElapsed < 1f)
        {
            timeElapsed += speed * Time.deltaTime;
            transform.localScale = startScale + _scaleUpCurve.Evaluate(timeElapsed) * scaleOffset;
            yield return null;
        }

        Destroy(gameObject);
    }
}
