export interface UserPreferences {
  favoriteCuisines: string[];
  favoriteDishTypes: string[];
  diets: string[];
  intolerances: string[];
  dislikedIngredientIds: string[];
}

export interface Ingredient {
  id: string;
  name: string;
}

export const CUISINES = [
  'African', 'Asian', 'American', 'British', 'Cajun', 'Caribbean', 'Chinese',
  'Eastern European', 'European', 'French', 'German', 'Greek', 'Indian',
  'Irish', 'Italian', 'Japanese', 'Jewish', 'Korean', 'Latin American',
  'Mediterranean', 'Mexican', 'Middle Eastern', 'Nordic', 'Southern',
  'Spanish', 'Thai', 'Vietnamese',
];

export const DISH_TYPES = [
  'Main Course', 'Side Dish', 'Dessert', 'Appetizer', 'Salad',
  'Bread', 'Breakfast', 'Soup', 'Beverage', 'Sauce', 'Marinade',
  'Fingerfood', 'Snack', 'Drink',
];

export const DIETS = [
  { value: 'gluten free', label: 'Gluten Free', description: 'Excludes gluten-containing grains' },
  { value: 'ketogenic', label: 'Ketogenic', description: 'Very low-carb, high-fat diet' },
  { value: 'vegetarian', label: 'Vegetarian', description: 'No meat, may include dairy/eggs' },
  { value: 'lacto-vegetarian', label: 'Lacto-Vegetarian', description: 'No meat/eggs, includes dairy' },
  { value: 'ovo-vegetarian', label: 'Ovo-Vegetarian', description: 'No meat/dairy, includes eggs' },
  { value: 'vegan', label: 'Vegan', description: 'No animal products' },
  { value: 'pescetarian', label: 'Pescetarian', description: 'Vegetarian plus fish/seafood' },
  { value: 'paleo', label: 'Paleo', description: 'Whole foods, no processed ingredients' },
  { value: 'primal', label: 'Primal', description: 'Similar to paleo, includes dairy' },
  { value: 'low fodmap', label: 'Low FODMAP', description: 'Limits fermentable carbohydrates' },
  { value: 'whole30', label: 'Whole30', description: '30-day elimination diet' },
];

export const INTOLERANCES = [
  { value: 'dairy', label: 'Dairy' },
  { value: 'egg', label: 'Egg' },
  { value: 'gluten', label: 'Gluten' },
  { value: 'grain', label: 'Grain' },
  { value: 'peanut', label: 'Peanut' },
  { value: 'seafood', label: 'Seafood' },
  { value: 'sesame', label: 'Sesame' },
  { value: 'shellfish', label: 'Shellfish' },
  { value: 'soy', label: 'Soy' },
  { value: 'sulfite', label: 'Sulfite' },
  { value: 'tree nut', label: 'Tree Nut' },
  { value: 'wheat', label: 'Wheat' },
];
