# Operatory

| Priorytet | Operator     | Opis                      | Arność  | Pozycja | Łączność     |
|-----------|--------------|---------------------------|---------|---------|--------------|
| 1         | ()           | wywołanie funkcji         | unarny  | sufiks  | lewostronna  |
|           | .            | dostęp do przestrzeni     | binarny | infiks  | lewostronna  |
| 2         | ^            | potęgowanie               | binarny | infiks  | prawostronna |
| 3         | +            | zachowanie znaku          | unarny  | prefiks | prawostronna |
|           | -            | zmiana znaku              | unarny  | prefiks | prawostronna |
|           | !            | negacja                   | unarny  | prefiks | prawostronna |
| 4         | *            | mnożenie                  | binarny | infiks  | lewostronna  |
|           | &#92;        | dzielenie                 | binarny | infiks  | lewostronna  |
|           | %            | reszta z dzielenia (rem)  | binarny | infiks  | lewostronna  |
| 5         | +            | dodawanie                 | binarny | infiks  | lewostronna  |
|           | -            | odejmowanie               | binarny | infiks  | lewostronna  |
| 6         | ..           | konkatenacja              | binarny | infiks  | lewostronna  |
| 7         | &lt;         | porównanie                | binarny | infiks  | lewostronna  |
|           | &lt;=        | porównanie                | binarny | infiks  | lewostronna  |
|           | &gt;         | porównanie                | binarny | infiks  | lewostronna  |
|           | &gt;=        | porównanie                | binarny | infiks  | lewostronna  |
| 8         | ==           | porównanie                | binarny | infiks  | lewostronna  |
|           | !=           | porównanie                | binarny | infiks  | lewostronna  |
| 9         | is           | sprawdzenie typu          | binarny | infiks  | lewostronna  |
|           | is not       | sprawdzenie typu          | binarny | infiks  | lewostronna  |
| 10        | &amp;&amp;   | koniunkcja                | binarny | infiks  | lewostronna  |
| 11        | &#124;&#124; | alternatywa               | binarny | infiks  | lewostronna  |
| 12        | ?&gt;        | null-safe pipe            | binarny | infiks  | lewostronna  |
| 13        | ??           | null coalescing           | binarny | infiks  | lewostronna  |
| 14        | =            | przypisanie               | binarny | infiks  | prawostronna |
|           | +=           | przypisanie sumy          | binarny | infiks  | prawostronna |
|           | -=           | przypisanie różnicy       | binarny | infiks  | prawostronna |
|           | *=           | przypisanie iloczynu      | binarny | infiks  | prawostronna |
|           | /=           | przypisanie ilorazu       | binarny | infiks  | prawostronna |
|           | %=           | przypisanie reszty        | binarny | infiks  | prawostronna |


## Operatory dopasowywania wzorca

| Priorytet | Operator | Opis             | Arność  | Pozycja | Łączność    |
|-----------|----------|------------------|---------|---------|-------------|
| 1         | &lt;     | porównanie       | unarny  | prefiks | brak        |
|           | &lt;=    | porównanie       | unarny  | prefiks | brak        |
|           | &gt;     | porównanie       | unarny  | prefiks | brak        |
|           | &gt;=    | porównanie       | unarny  | prefiks | brak        |
|           | ==       | porównanie       | unarny  | prefiks | brak        |
|           | !=       | porównanie       | unarny  | prefiks | brak        |
| 2         | is       | sprawdzenie typu | unarny  | prefiks | brak        |
|           | is not   | sprawdzenie typu | unarny  | prefiks | brak        |
| 3         | and      | koniunkcja       | binarny | infiks  | lewostronna |
| 4         | or       | altenatywa       | binarny | infiks  | lewostronna |
