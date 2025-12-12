const joi = require('joi');

// Validation schemas
const registerSchema = joi.object({
  username: joi
    .string()
    .alphanum()
    .min(3)
    .max(20)
    .required()
    .messages({
      'string.alphanum': 'Username must only contain letters and numbers',
      'string.min': 'Username must be at least 3 characters',
      'string.max': 'Username must not exceed 20 characters',
      'any.required': 'Username is required',
    }),
  email: joi
    .string()
    .email()
    .required()
    .messages({
      'string.email': 'Please provide a valid email',
      'any.required': 'Email is required',
    }),
  password: joi
    .string()
    .min(6)
    .required()
    .messages({
      'string.min': 'Password must be at least 6 characters',
      'any.required': 'Password is required',
    }),
});

const loginSchema = joi.object({
  email: joi
    .string()
    .email()
    .required()
    .messages({
      'string.email': 'Please provide a valid email',
      'any.required': 'Email is required',
    }),
  password: joi
    .string()
    .required()
    .messages({
      'any.required': 'Password is required',
    }),
});

const updateUserSchema = joi.object({
  username: joi
    .string()
    .alphanum()
    .min(3)
    .max(20)
    .optional(),
  email: joi
    .string()
    .email()
    .optional(),
  profileImage: joi
    .string()
    .uri()
    .optional()
    .allow(null),
});

// Validate function
const validate = (data, schema) => {
  const { error, value } = schema.validate(data, {
    abortEarly: false,
    stripUnknown: true,
  });

  if (error) {
    const messages = error.details.map(detail => ({
      field: detail.path[0],
      message: detail.message,
    }));
    return { valid: false, errors: messages };
  }

  return { valid: true, data: value };
};

module.exports = {
  registerSchema,
  loginSchema,
  updateUserSchema,
  validate,
};
