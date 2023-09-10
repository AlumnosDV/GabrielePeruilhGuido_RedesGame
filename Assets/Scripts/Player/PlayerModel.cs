using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerModel : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float _life;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _jumpForce;
    
    [SerializeField] private Rigidbody2D _myRigidBody;
    private bool _isJumping = false;
    private float _moveHorizontal;
    private float _moveVertical;
    private int _currentSign, _previousSign;

    private void Awake()
    {
        _myRigidBody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        transform.right = Vector2.right;
    }

    void Update()
    {
        _moveHorizontal = Input.GetAxis("Horizontal");
        _moveVertical = Input.GetAxis("Vertical");
        if (Input.GetKeyDown(KeyCode.W) && !_isJumping)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (_moveHorizontal > 0.1f || _moveHorizontal < -0.1f)
        {
            //_myRigiBody.MovePosition(transform.position + Vector3.right * (_moveHoritonzatal * _moveSpeed * Time.fixedDeltaTime));
            _myRigidBody.AddForce(new Vector2(_moveHorizontal * _moveSpeed, 0f), ForceMode2D.Impulse);

            _currentSign = (int)Mathf.Sign(_moveHorizontal);
            Debug.Log(_currentSign);
            if (_currentSign != _previousSign)
            {
                _previousSign = _currentSign;

                transform.right = Vector2.right * _currentSign;
            }
            _animator.SetFloat("HorizontalValue", Mathf.Abs(_moveHorizontal));
        }
    }
    
    void Jump()
    {
        Debug.Log(Vector2.up);
        _myRigidBody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
    }
    

    public void TakeDamage(float dmg)
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Floor"))
            _isJumping = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Floor"))
            _isJumping = true;
    }
}
