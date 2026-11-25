
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class DOTweenShowcaseController : MonoBehaviour
{
    [Header("Escenario 1: Salto")]
    [SerializeField] private Transform jumpCube;

    [Header("Escenario 2: Interacción")]
    [SerializeField] private Transform redCube;
    [SerializeField] private Transform blueCube;

    [Header("Interfaz")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private TMP_Text statusText;

    [Header("Configuración de animaciones")]
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpDuration = 1.5f;

    // Posiciones iniciales
    private Vector3 jumpStartPosition;
    private Vector3 redStartPosition;
    private Vector3 blueStartPosition;

    // Escalas iniciales
    private Vector3 jumpStartScale;
    private Vector3 redStartScale;
    private Vector3 blueStartScale;

    // Rotaciones iniciales
    private Quaternion jumpStartRotation;
    private Quaternion redStartRotation;
    private Quaternion blueStartRotation;

    // Colores iniciales
    private Color jumpStartColor;
    private Color redStartColor;
    private Color blueStartColor;

    // Secuencias de DOTween
    private Sequence jumpSequence;
    private Sequence interactionSequence;
    private Sequence timeDilationSequence;

    // Componentes Renderer
    private Renderer jumpRenderer;
    private Renderer redRenderer;
    private Renderer blueRenderer;

    // Estado de las animaciones
    private bool isPlaying;
    private bool jumpFinished;
    private bool interactionFinished;

    private void Awake()
    {
        SaveInitialValues();

        // Conectar los botones de la interfaz
        playButton.onClick.AddListener(PlayAnimations);
        resetButton.onClick.AddListener(ResetAnimations);
    }

    private void SaveInitialValues()
    {
        // Guardar las posiciones iniciales
        jumpStartPosition = jumpCube.position;
        redStartPosition = redCube.position;
        blueStartPosition = blueCube.position;

        // Guardar las escalas iniciales
        jumpStartScale = jumpCube.localScale;
        redStartScale = redCube.localScale;
        blueStartScale = blueCube.localScale;

        // Guardar las rotaciones iniciales
        jumpStartRotation = jumpCube.rotation;
        redStartRotation = redCube.rotation;
        blueStartRotation = blueCube.rotation;

        // Obtener los componentes Renderer
        jumpRenderer = jumpCube.GetComponent<Renderer>();
        redRenderer = redCube.GetComponent<Renderer>();
        blueRenderer = blueCube.GetComponent<Renderer>();

        // Guardar los colores iniciales
        jumpStartColor = jumpRenderer.material.color;
        redStartColor = redRenderer.material.color;
        blueStartColor = blueRenderer.material.color;
    }

    // REPRODUCIR ANIMACIONES

    public void PlayAnimations()
    {
        if (isPlaying)
            return;

        ResetAnimations();

        isPlaying = true;

        jumpFinished = false;
        interactionFinished = false;

        playButton.interactable = false;
        statusText.text = "ANIMATING...";

        CreateJumpSequence();
        CreateInteractionSequence();
    }

    // ESCENARIO 1: SALTO

    private void CreateJumpSequence()
    {
        jumpSequence = DOTween.Sequence();

        // Estirar ligeramente el cubo antes del salto
        jumpSequence.Append(jumpCube.DOScale(new Vector3(0.65f, 1.1f, 0.65f), 0.15f));

        // Realizar el salto y aterrizar en la posición inicial
        jumpSequence.Append(jumpCube.DOJump(jumpStartPosition, jumpHeight, 1, jumpDuration).SetEase(Ease.OutQuad));

        // Recuperar la escala original durante el salto
        jumpSequence.Join(jumpCube.DOScale(jumpStartScale, jumpDuration));

        // Comprimir el cubo al aterrizar
        jumpSequence.Append(jumpCube.DOScale(new Vector3(1f, 0.7f, 1f), 0.12f));

        // Recuperar la forma y el color originales
        jumpSequence.Append(jumpCube.DOScale(jumpStartScale, 0.2f).SetEase(Ease.OutBack));

        jumpSequence.Join(jumpRenderer.material.DOColor(jumpStartColor, 0.2f));

        // Indicar que la animación del salto ha terminado
        jumpSequence.OnComplete(() =>
        {
            jumpFinished = true;
            CheckAnimationsCompleted();
        });
    }

    // ESCENARIO 2: INTERACCIÓN

    private void CreateInteractionSequence()
    {
        interactionSequence = DOTween.Sequence();

        // 1. El cubo rojo se acerca al cubo azul
        interactionSequence.Append(redCube.DOMoveX(redStartPosition.x + 0.8f, 0.6f).SetEase(Ease.OutBack));

        // 2. El cubo azul se desplaza simultáneamente
        interactionSequence.Join(blueCube.DOMoveX(blueStartPosition.x - 0.2f, 0.6f).SetEase(Ease.OutBack));

        // 3. El cubo rojo realiza una rotación
        interactionSequence.Append(redCube.DORotate(redStartRotation.eulerAngles + new Vector3(0f, 180f, 0f), 0.6f).SetEase(Ease.InOutSine));

        // 4. El cubo azul realiza un salto
        interactionSequence.Append(blueCube.DOJump(blueStartPosition, 1.5f, 1, 0.8f));

        // Aumentar la escala del cubo azul durante el salto
        interactionSequence.Join(blueCube.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.8f));

        // Activar la dilatación temporal antes del impacto
        interactionSequence.AppendCallback(() =>
        {
            PlayTimeDilation();
        });

        // 5. El cubo rojo realiza un movimiento de impacto
        interactionSequence.Append(redCube.DOPunchPosition(Vector3.up * 0.8f, 0.6f, 5, 0.7f));

        // 6. Ambos cubos regresan a sus posiciones iniciales
        interactionSequence.Append(redCube.DOMove(redStartPosition, 0.8f).SetEase(Ease.InOutBack));

        interactionSequence.Join(blueCube.DOMove(blueStartPosition, 0.8f).SetEase(Ease.InOutBack));

        // 7. Recuperar las escalas originales
        interactionSequence.Append(redCube.DOScale(redStartScale, 0.3f));

        interactionSequence.Join(blueCube.DOScale(blueStartScale, 0.3f));

        // Recuperar la rotación original del cubo rojo
        interactionSequence.Join(redCube.DORotate(redStartRotation.eulerAngles, 0.3f));

        // 8. Cambiar temporalmente el color de ambos cubos
        interactionSequence.Append(redRenderer.material.DOColor(Color.yellow, 0.15f));

        interactionSequence.Join(blueRenderer.material.DOColor(Color.white, 0.15f));

        // Recuperar los colores originales
        interactionSequence.Append(redRenderer.material.DOColor(redStartColor, 0.2f));

        interactionSequence.Join(blueRenderer.material.DOColor(blueStartColor, 0.2f));

        // Indicar que la secuencia de interacción ha terminado
        interactionSequence.OnComplete(() =>
        {
            interactionFinished = true;
            CheckAnimationsCompleted();
        });
    }

    // DILATACIÓN TEMPORAL

    private void PlayTimeDilation()
    {
        // Detener cualquier dilatación temporal anterior
        if (timeDilationSequence != null)
            timeDilationSequence.Kill();

        Time.timeScale = 1f;

        timeDilationSequence = DOTween.Sequence();

        // Reducir progresivamente la velocidad del tiempo
        timeDilationSequence.Append(DOTween.To(() => Time.timeScale, value => Time.timeScale = value, 0.35f, 0.25f));

        // Recuperar progresivamente la velocidad normal
        timeDilationSequence.Append(DOTween.To(() => Time.timeScale, value => Time.timeScale = value, 1f, 0.6f));

        // Ejecutar esta secuencia independientemente del Time.timeScale
        timeDilationSequence.SetUpdate(true);

        timeDilationSequence.OnComplete(() =>
        {
            Time.timeScale = 1f;
        });
    }

    // COMPROBAR FINALIZACIÓN
    private void CheckAnimationsCompleted()
    {
        // Comprobar que ambos escenarios han terminado
        if (jumpFinished && interactionFinished)
        {
            isPlaying = false;

            playButton.interactable = true;

            statusText.text = "COMPLETED";
        }
    }

    // REINICIAR ANIMACIONES

    public void ResetAnimations()
    {
        // Detener la dilatación temporal
        if (timeDilationSequence != null)
        {
            timeDilationSequence.Kill();
            timeDilationSequence = null;
        }

        Time.timeScale = 1f;

        // Detener la secuencia del salto
        if (jumpSequence != null)
        {
            jumpSequence.Kill();
            jumpSequence = null;
        }

        // Detener la secuencia de interacción
        if (interactionSequence != null)
        {
            interactionSequence.Kill();
            interactionSequence = null;
        }

        // Detener las animaciones individuales
        jumpCube.DOKill();
        redCube.DOKill();
        blueCube.DOKill();

        jumpRenderer.material.DOKill();
        redRenderer.material.DOKill();
        blueRenderer.material.DOKill();

        // Restaurar las posiciones iniciales
        jumpCube.position = jumpStartPosition;
        redCube.position = redStartPosition;
        blueCube.position = blueStartPosition;

        // Restaurar las escalas iniciales
        jumpCube.localScale = jumpStartScale;
        redCube.localScale = redStartScale;
        blueCube.localScale = blueStartScale;

        // Restaurar las rotaciones iniciales
        jumpCube.rotation = jumpStartRotation;
        redCube.rotation = redStartRotation;
        blueCube.rotation = blueStartRotation;

        // Restaurar los colores iniciales
        jumpRenderer.material.color = jumpStartColor;
        redRenderer.material.color = redStartColor;
        blueRenderer.material.color = blueStartColor;

        // Reiniciar el estado de las animaciones
        isPlaying = false;
        jumpFinished = false;
        interactionFinished = false;

        // Restaurar los elementos de la interfaz
        if (playButton != null)
            playButton.interactable = true;

        if (statusText != null)
            statusText.text = "READY";
    }

    // LIMPIEZA

    private void OnDestroy()
    {
        // Detener todas las secuencias creadas por este controlador
        if (timeDilationSequence != null)
            timeDilationSequence.Kill();

        if (jumpSequence != null)
            jumpSequence.Kill();

        if (interactionSequence != null)
            interactionSequence.Kill();

        // Recuperar la velocidad normal del juego
        Time.timeScale = 1f;
    }
}