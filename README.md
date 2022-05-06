# Toffee

Interpreter docelowo prostego i przyjemnego w użyciu języka skryptowego ogólnego przeznaczenia o roboczej nazwie Toffee.  
Język implementacji: C#.

Głównymi założeniami projektowanego języka są:
* wykorzystanie dynamicznego, słabego typowania,
* opcjonalność i domyślna mutowalność zmiennych,
* implementacja dopasowywania wzorca.

Przeważająca większość konstruktów językowych to wyrażenia, co ma uczynić pracę z językiem wygodniejszą.    
Obejmuje to m.in.:
* bloki kodu, które zwracają wartość ostatniego wyrażenia w nich zawartego (lub `null`, jeśli są puste),
* pętle `for` i `while` zwracające po zakończeniu wartość odpowiednio licznika oraz sprawdzanego warunku,
* przypisania wartości do zmiennych.

Język implementuje funkcje anonimowe i pozwala na przypisywanie ich do zmiennych.

Podstawowe typy danych będą przekazywane do funkcji przez kopię, natomiast łańcuchy znaków (których zawartość jest niezmienna) poprzez referencję.


## Formalna specyfikacja i składnia

Gramatyka realizowanego języka opisana jest w pliku [gramatyka.md](docs/gramatyka.md). Reguły dotyczące operatorów są zgodne z tabelami z pliku [operatory.md](docs/operatory.md).

Nie przewiduje się na razie konfiguracji zachowania interpretera poprzez specjalne pliki.

Możliwe jest importowanie zawartości innych skryptów za pomocą instrukcji `pull`. Rolę biblioteki standardowej pełni przestrzeń nazw `std`. Zdefiniowane w "dociąganym" skrypcie elementy są wprowadzanie do przestrzeni nazw skryptu głównego, a w wypadku konfliktu nazw można odwołać się do nich z użyciem pełnej ścieżki (np. `std.io.print`).

## Wymaganie funkcjonalne
1. typy
    * rodzaje:
        * `int` (całkowitoliczbowy)
        * `float` (zmiennoprzecinkowy)
        * `string` (znakowy)
        * `bool` (logiczny)
        * `function` (funkcyjny)
        * `null` (brak wartości)
    * typ wyrażenia można sprawdzić z użyciem operatorów: `is`, `is not`
2. obsługa operacji liczbowych
    * dwa typy podstawowe: liczby całkowite ze znakiem (`int`, od $`-2^{63}`$ do $`2^{63}-1`$) i liczby zmiennoprzecinkowe (`float`, IEEE 754 binary64)
    * obsługa literałów całkowitoliczbowych w formie dziesiętnej (np. `3424`), szesnastkowej (np. `0xaf`), ósemkowej (np. `0x644`) oraz dwójkowej (`0b101011`)
    * obsługa literałów zmiennoprzecinkowych z opcjonalną częścią ułamkową, ale nie całkowitą (np. `25.`, ale nie `.1234`) oraz wsparciem dla notacji naukowej bez znormalizowanej mantysy (np. `12.34e15`)
    * operatory: znaku (`+`, `-`), dodawania (`+`), odejmowania (`-`), mnożenia (`*`), dzielenia (`/`), reszty z dzielenia (`%`), potęgowania (`^`)
3. obsługa operacji znakowych
    * typ `string`
    * wieloliniowe literały ograniczone cudzysłowami wspierające sekwencje ucieczki z wykorzystanie znaku `\`
        * dosłowne sekwencje nowej linii w treści zamieniane są na znaki `\n`
        * zakładane wsparcie dla standardowych dla języków z rodziny C znaków ucieczki (m.in. `\r`, `\n`, `\t`, `\0`)
    * operatory: konkatenacji (`..`)
4. obsługa komentarzy
    * jednoliniowych od sekwencji `//`
    * wieloliniowych od sekwencji `/*` do sekwencji `*/` (bez zagnieżdżeń)
5. tworzenie zmiennych
    * semantyka obsługi zmiennych: typowanie dynamiczne, słabe; opcjonalność; domyślna mutowalność
    * słowo kluczowe `init` rozpoczynające listę deklaracji oraz `const` wskazujące stałą
    * deklaracja oznacza inicjalizację - jeśli nie określono przypisania, zmienna przyjmuje wartość `null` (nie dotyczy stałych)
    * obsługa zakresów widoczności zmiennych ze wsparciem dla domknięć (wartość przechwyconych zmiennych nie może być zmieniana przez domknięcie)
6. instrukcje warunkowe
    * instrukcja warunkowa `if`
        * opcjonalne części `elif` (wiele wystąpień) oraz `else` (jedno wystąpienie)
        * dla `if` oraz `elif` wymagane jest zdefiniowanie warunku w nawiasach
7. instrukcje pętli
    * instrukcja pętli zakresowej `for`
        * pozwala na zadeklarowanie niemutowalnego licznika (o wybranej nazwie) inkrementowanego wedle specyfikacji zakresu; deklaracja ta jest opcjonalna (pętla wykona się poprawną ilość razy bez konieczności deklaracji)
        * specyfikacja zakresu umieszczona jest w nawiasach po słowie kluczowym `for`, działa analogicznie do konstrukcji `range` z języka Python: możliwe jest określenie tylko górnej granicy (np. `5`), wartości startowej i górnej granicy (np. `0:5`) lub wartości startowej, górnej granicy i kroku inkrementacji (np. `0:5:2`)
    * instrukcja pętli warunkowej `while`
        * "klasyczna" postać - wymaga podania w nawiasach po słowie kluczowym `while` jakiegoś warunku ewaluowanego przed każdą iteracją
    * przerwanie wykonania
        * słowo kluczowe `break` pozwala na bezwarunkowe przerwanie wykonania obu typów pętli
        * słowo kluczowe `break_if` pozwala na warunkowe przerwanie wykonania obu typów pętli - warunek należy podać w nawiasach
8. funkcje
    * defiowanie funkcji anonimowych z użyciem słowa kluczowego `functi`, po którym następuje lista parametrów i ciało funkcji (blok)
    * funkcje anonimowe mogą być przypisane do zmiennej/stałej
    * funkcje anonimowe mogą przechwytywać zmienne (mechanizm domknięć), ale nie mogą ich modyfikować
    * wywołanie funkcji możliwe jest z użyciem nawiasów, w których podane są argumenty, możliwe rekursywne wywołania
    * proste typy danych (liczbowe, logiczne) są przekazywane do funkcji przez kopię, natomiast łańcuchy znaków (których zawartość jest niezmienna) poprzez referencję
    * możliwe jest wymuszenie sprawdzenia nieopcjonalności parametru z użyciem operatora sufiksowego `!`
    * parametry funkcji można określić słowem kluczowym `const` - nie będzie się dało wtedy modyfikować ich wartości (domyślnie są mutowalne)
    * przerwanie wykonania funkcji można wymusić z użyciem słowa kluczowe `return`, po którym następić może wyrażenie stanowiące wartość zwrotną
9. obsługa błędów
    * z użyciem domyślnej implementacji: wypisywanie błędów w określonym formacie (zawierającym pozycję, oznaczenie błędu i dodatkowe dane) na standardowy strumień błędów `stderr` na każdym etapie działania aplikacji (błędy znakowe, składniowe, semantyczne, czasu uruchomienia)
    * przykładowe błędy:
        * znakowe - nieoczekiwany znak (w szczególności ETX)
        * składniowe - nieprawidłowa instrukcja, nieoczekiwany token (w szczególności ETX), przekroczenie zakresu literału
        * semantyczne - przypisanie wartości do zdefiniowanej wcześniej stałej, nieznany identyfikator, brak przypisania przy definicji stałej
        * czasu uruchomienia - dzielenie przez zero, `null` podany w miejsce parametru nieopcjonalnego, przekroczenie zakresu zmiennej, brak wymaganej przestrzeni nazw
    * wystąpienie błędu oznacza przerwanie działania bieżącego programu (w przypadku trybu REPL nie powinno to kończyć działania interperetera)
        * w przypadku błędów składniowych należy pominąć wszelkie tokeny aż do rozpoczęcia następnej instrukcji i kontynuować sprawdzanie, by użytkownik mógł zapoznać się z możliwie pełną liczbą błędów od razu
    * wygodny byłby mechanizm wyjątków z użyciem słów kluczowych `try` i `catch` z możliwością definiowania własnych klas błędów, nie jest to jednak obecnie zaplanowane (wiązałoby się z potencjalnym wprowadzeniem mechanizmu dziedziczenia, wsparcia dla OOP, itd.)
10. obsługa operacji logicznych
    * typ `bool`
    * obsługa literałów: `true`, `false`
    * operatory: negacja (`!`), iloczyn logiczny (`&&`), alternatywa (`||`)
    * porównanie wartości (`<`, `<=`, `>`, `>=`, `==`, `!=`) działa jak w języku Python - do momentu zamknięcia wyrażenie będącego ciągiem porównań, wykorzystywana jest porównywana wartość zmiennych, literałów, itp., po zamknięciu wynik stanowi wartość logiczna stanowiąca iloczyn logiczny wyników wszystkich składowych porównań
11. dopasowanie wzorca
    * z pattern matchingu można skorzystać przy pomocy słowa kluczowego `match`
    * obsługiwane dopasowania:
        * sprawdzenie typu (operatory `is`, `is not`)
        * porównanie z literałem (operatory `==`, `!=`, `<`, `<=`, `>`, `>=`)
        * spełnienie predykatu (poprzez podanie nazwy jednoargumentowej funkcji)
    * warunki można grupować za pomocą nawaisów oraz słów kluczowych: `and`, `or`
12. obsługa opcjonalności
    * każda zmienna może przyjąć wartość `null` (`null` jest osobnego typu)
    * najprostszą operacją możliwą do przeprowadzenia na wartości opcjonalnej jest dostarczenie w wyrażeniu wartości "awaryjnej" na wypadek wystąpienia nulla za pomocą operatora binarnego `??`
    * możliwa jest również wykonywanie szeregu operacji, dopóki wartość nie stanie się nullem, za pomocą operatora potoku `?>`

## Wymagania niefunkcjonalne

1. Lekser i parser powinny działać na tyle szybko i sprawnie, by możliwe było wyświetlanie informacji np. o błędach składniowych na żywo w trakcie pisania programu.
2. Interpreter powinien być odporny na błędy - nie zawieszać się, nie przerywać nagle działania.

## Sposób uruchomienia

Interpreter będzie dostarczony w postaci programu konsolowego. Uruchomiony bez argumentów, będzie pobierał dane ze standardowego strumienia wejściowego. Jako argumenty podać można natomiast pliki skryptowe, które zostaną wczytane i wykonane. W takim przypadku standardowe wejście pozostanie nieczynne. 

## Architektura

System składał się będzie z następujących warstw:
* skaner znaków (leniwa generacja znaków) - śledzenie pozycji, unifikacja znaków nowej linii,
* analizator leksykalny (leniwa generacja tokenów),
* analizator składniowy - parser typu RD generujący hierarchię obiektów,
* interpreter wygenerowanego drzewa.

Dodatkowo zaimplementowane będą różne klasy do obsługi błędów, logowania, itp.

## Testowanie

Analizator leksykalny oraz składniowy pracują na zasadzie konsumpcji kolejnych znaków (lub tokenów) i generowania rezultatu, który powinien być deterministyczny, a więc i możliwy do sprawdzenia pod kątem poprawności (np. poprzez proste porównanie sekwencji).

Komponenty systemu testowane będą jednostkowo, niezależnie dzięki wykorzystaniu w implementacji podejścia obiektowego (możliwe będzie podstawienie obiektu np. leksera z użyciem atrapy).

Oczywiście testowane będą, poza przykładami mającymi zadziałać poprawnie, przypadki brzegowe, np. nagłe przerwanie strumienia wejściowego, dzielenie przez zero i wszelkie błędy, zaproponowane wyżej lub nie.
