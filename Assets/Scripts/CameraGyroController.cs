using UnityEngine;

public class CameraGyroController : MonoBehaviour
{
    public Transform target; // O objeto ao redor do qual a câmera vai girar

    private Vector3 initialOffset; // Offset inicial da câmera ao objeto

    void Start() {
        // Ativa o giroscópio
        Input.gyro.enabled = true;

        if (target == null) {
            Debug.LogError("Target não atribuído.");
            return;
        }

        // Calcula o offset inicial com base na posição atual da câmera e do objeto alvo
        initialOffset = transform.position - target.position;
    }

    void Update() {
        if (target == null) {
            return;
        }

        // Obtém a rotação do giroscópio
        Quaternion deviceRotation = Input.gyro.attitude;

        // Cria uma nova rotação no eixo Y
        Quaternion yRotation = Quaternion.Euler(0, -deviceRotation.eulerAngles.z, 0);

        // Aplica a rotação ao offset inicial
        Vector3 rotatedOffset = yRotation * initialOffset;

        // Atualiza a posição da câmera mantendo a distância ao objeto alvo
        transform.position = target.position + rotatedOffset;
        transform.LookAt(target); // Garante que a câmera esteja sempre olhando para o objeto alvo
    }
}
