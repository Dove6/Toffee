# Operatory

| Pierwszeństwo | Operator | Opis                      | Arność  | Pozycja | Łączność     |
|---------------|----------|---------------------------|---------|---------|--------------|
|       1       | ()       | wywołanie funkcji         | unarny  | sufiks  | lewostronna  |
|               | .        | dostęp do przestrzeni     | binarny | infiks  | lewostronna  |
|               | ?.       | bezpieczny dostęp         | binarny | infiks  | lewostronna  |
|               | !        | wykluczenie opcjonalności | unarny  | sufiks  | lewostronna  |
|       2       | ^        | potęgowanie               | binarny | infiks  | prawostronna |
|       3       | +        | zachowanie znaku          | unarny  | prefiks | prawostronna |
|               | -        | zmiana znaku              | unarny  | prefiks | prawostronna |
|               | !        | negacja                   | unarny  | prefiks | prawostronna |
|       4       | *        | mnożenie                  | binarny | infiks  | lewostronna  |
|               | /        | dzielenie                 | binarny | infiks  | lewostronna  |
|               | %        | remainder                 | binarny | infiks  | lewostronna  |
|       5       | +        | dodawanie                 | binarny | infiks  | lewostronna  |
|               | -        | odejmowanie               | binarny | infiks  | lewostronna  |
|       6       | ..       | konkatenacja              | binarny | infiks  | lewostronna  |
|       7       | <        | porównanie                | binarny | infiks  | lewostronna  |
|               | <=       | porównanie                | binarny | infiks  | lewostronna  |
|               | >        | porównanie                | binarny | infiks  | lewostronna  |
|               | >=       | porównanie                | binarny | infiks  | lewostronna  |
|       8       | ==       | porównanie                | binarny | infiks  | lewostronna  |
|               | !=       | porównanie                | binarny | infiks  | lewostronna  |
|       9       | &&       | koniunkcja                | binarny | infiks  | lewostronna  |
|      10       | \|\|     | altenatywa                | binarny | infiks  | lewostronna  |
|      11       | ??       | null coalescing           | binarny | infiks  | lewostronna  |
|      12       | =        | przypisanie               | binarny | infiks  | prawostronna |
|               | +=       | przypisanie sumy          | binarny | infiks  | prawostronna |
|               | -=       | przypisanie różnicy       | binarny | infiks  | prawostronna |
|               | *=       | przypisanie iloczynu      | binarny | infiks  | prawostronna |
|               | /=       | przypisanie ilorazu       | binarny | infiks  | prawostronna |
|               | %=       | przypisanie reszty        | binarny | infiks  | prawostronna |


## Operatory dopasowywania wzorca

| Pierwszeństwo | Operator | Opis                      | Arność  | Pozycja | Łączność     |
|---------------|----------|---------------------------|---------|---------|--------------|
|       1       | <        | porównanie                | unarny  | prefiks | brak         |
|               | <=       | porównanie                | unarny  | prefiks | brak         |
|               | >        | porównanie                | unarny  | prefiks | brak         |
|               | >=       | porównanie                | unarny  | prefiks | brak         |
|               | is       | porównanie                | unarny  | prefiks | brak         |
|               | is not   | porównanie                | unarny  | prefiks | brak         |
|       3       | and      | koniunkcja                | binarny | infiks  | lewostronna  |
|       4       | or       | altenatywa                | binarny | infiks  | lewostronna  |
