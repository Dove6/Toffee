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
* przypisania wartości do zmiennych (nie chodzi tu o deklaracje).

Język implementuje funkcje anonimowe i pozwala na przypisywanie ich do zmiennych.

Argumenty są przekazywane do funkcji przez kopię.


## Formalna specyfikacja i składnia

Gramatyka realizowanego języka opisana jest w pliku [gramatyka.md](docs/gramatyka.md). Reguły dotyczące operatorów są zgodne z tabelami z pliku [operatory.md](docs/operatory.md).

Nie przewiduje się konfiguracji zachowania interpretera poprzez specjalne pliki.

Planowane było umożliwienie użytkownikowi importowania zawartości innych skryptów za pomocą instrukcji `pull`. Rolę biblioteki standardowej miała pełnić przestrzeń nazw `std`. Zdefiniowane w "dociąganym" skrypcie elementy miały być wprowadzane do przestrzeni nazw skryptu głównego, a w wypadku konfliktu nazw możliwe miało być odwołanie się do nich z użyciem pełnej ścieżki (np. `std.io.print`).

Ze względu na obecny brak wsparcia (w klasie interpretera) dla instrukcji `pull`, funkcje wbudowane `print` oraz `quit` zlokalizowane są bezpośrednio w środowisku użytkownika.

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
    * przepełnienie podczas operacji na liczbach całkowitych nie jest zgłaszane
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
        * warunek jest spełniony, jeśli jego wartość rzutowana do typu `bool` wynosi `true`
7. instrukcje pętli
    * instrukcja pętli zakresowej `for`
        * pozwala na zadeklarowanie niemutowalnego licznika (o wybranej nazwie) inkrementowanego wedle specyfikacji zakresu; deklaracja ta jest opcjonalna (pętla wykona się poprawną ilość razy bez konieczności deklaracji)
        * specyfikacja zakresu umieszczona jest w nawiasach po słowie kluczowym `for`, działa analogicznie do konstrukcji `range` z języka Python: możliwe jest określenie tylko górnej granicy (np. `5`), wartości startowej i górnej granicy (np. `0:5`) lub wartości startowej, górnej granicy i kroku inkrementacji (np. `0:5:2`)
    * instrukcja pętli warunkowej `while`
        * "klasyczna" postać - wymaga podania w nawiasach po słowie kluczowym `while` jakiegoś warunku ewaluowanego przed każdą iteracją
        * warunek jest spełniony, jeśli jego wartość rzutowana do typu `bool` wynosi `true`
    * przerwanie wykonania
        * słowo kluczowe `break` pozwala na bezwarunkowe przerwanie wykonania obu typów pętli
        * słowo kluczowe `break_if` pozwala na warunkowe przerwanie wykonania obu typów pętli - warunek należy podać w nawiasach
8. funkcje
    * defiowanie funkcji anonimowych z użyciem słowa kluczowego `functi`, po którym następuje lista parametrów i ciało funkcji
    * funkcje anonimowe mogą być przypisane do zmiennej/stałej
    * funkcje anonimowe mogą przechwytywać zmienne (mechanizm domknięć), ale nie mogą ich modyfikować
    * wywołanie funkcji możliwe jest z użyciem nawiasów, w których podane są argumenty, możliwe rekursywne wywołania
    * proste typy danych (liczbowe, logiczne) są przekazywane do funkcji przez kopię, natomiast łańcuchy znaków (których zawartość jest niezmienna) poprzez referencję
    * możliwe jest wymuszenie sprawdzenia nieopcjonalności parametru z użyciem operatora sufiksowego `!`
    * parametry funkcji można określić słowem kluczowym `const` - nie będzie się dało wtedy modyfikować ich wartości (domyślnie są mutowalne)
    * przerwanie wykonania funkcji można wymusić z użyciem słowa kluczowe `return`, po którym następić może wyrażenie stanowiące wartość zwrotną
9. obsługa błędów
    * z użyciem domyślnej implementacji: wypisywanie błędów w określonym formacie (zawierającym pozycję, oznaczenie błędu i dodatkowe dane) na standardowy strumień błędów `stderr` (lub inny wybrany) na każdym etapie działania aplikacji (błędy leksykalne, składniowe, czasu uruchomienia)
    * zaimplementowane błędy:
        * leksykalne - nieoczekiwany znak (w szczególności koniec strumienia), przekroczenie maksymalnej długości tokena (komentarza, łańcucha znaków lub liczby), nieprawidłowy przedrostek liczbowy, brakująca część liczby (wykładnik, po przedrostku), nieznany token
        * składniowe - nieoczekiwany token (w szczególności ETX), wyrażenie lub instrukcja (w miejscu innego tokena, wyrażenia, instrukcji), przekroczenie zakresu liczby całkowitej, niepoprawne użycie gałęzi `default` w dopasowaniu wzorca (podwojenie, nieumieszczenie na ostatniej pozycji), brak wartości początkowej dla zmiennej niemutowalnej, użycie tej samej nazwy parametru więcej niż raz
        * czasu uruchomienia - przypisanie wartości do zdefiniowanej wcześniej stałej, nieznany identyfikator, dzielenie przez zero, `null` podany w miejsce parametru nieopcjonalnego, ponowna inicjalizacja zmiennej, nieprawidłowe rzutowanie, użycie instrukcji `return` poza funkcją, użycie instrukcji `break` poza pętlą, wywołanie funkcji z nieprawidłową liczba argumentów, wyrażenie przyjmujące wartość `null` w definicji zakresu pętli `for`, nieprawidłowa l-wartość w przypisaniu
    * wystąpienie błędu czasu uruchomienia oznacza przerwanie działania interpretera i ograniczenie się do sparsowania reszty tekstu (nie dotyczy to trybu REPL, gdzie błędy czasu uruchomienia są ignorowane)
        * w przypadku niemożliwych do rozwiązania błędów składniowych pomijane są wszelkie tokeny aż do rozpoczęcia następnej instrukcji i kontynuować sprawdzanie, by użytkownik mógł zapoznać się z możliwie pełną liczbą błędów od razu
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
    * warunki można grupować za pomocą nawiasów oraz słów kluczowych: `and`, `or`
12. obsługa opcjonalności
    * każda zmienna może przyjąć wartość `null` (`null` jest osobnego typu)
    * najprostszą operacją możliwą do przeprowadzenia na wartości opcjonalnej jest dostarczenie w wyrażeniu wartości "awaryjnej" na wypadek wystąpienia nulla za pomocą operatora binarnego `??`
    * możliwa jest również wykonywanie szeregu operacji, dopóki wartość nie stanie się nullem, za pomocą operatora potoku `?>`

## Wymagania niefunkcjonalne

1. Lekser i parser powinny działać na tyle szybko i sprawnie, by możliwe było wyświetlanie informacji np. o błędach składniowych na żywo w trakcie pisania programu.
2. Interpreter w trybie REPL powinien być odporny na błędy - nie zawieszać się, nie przerywać nagle działania.

## Sposób uruchomienia

Interpreter jest dostarczony w postaci programu konsolowego. Uruchomiony bez argumentów, pobiera dane ze standardowego strumienia wejściowego. Jako argumenty podać można natomiast plik skryptowy, który zostanie wczytany i wykonany. W takim przypadku standardowe wejście pozostanie nieczynne.

Do zbudowania i uruchomienia programu wymagane jest środowisko .NET w wersji >=6.  
Po wejściu do katalogu Toffee należy uruchomić polecenie `dotnet run`.

## Architektura

System składa się z następujących warstw:
* skaner znaków (leniwa generacja znaków) - śledzenie pozycji, unifikacja znaków nowej linii,
* analizator leksykalny (leniwa generacja tokenów),
* analizator składniowy - parser typu RD generujący abstrakcyjne drzewo składniowe (leniwa generacja instrukcji),
* interpreter wygenerowanego drzewa.

Dodatkowo zaimplementowane są różne klasy pomocnicze:
* katalog CommandLine - nadzorowanie uruchomienia interpretera (tryb REPL lub nie),
* katalog ErrorHandling - obsługa błędów,
* katalog Running/Operations - operacje czasu uruchomienia (np. arytmetyczne czy rzutowanie),
* katalog Running/Functions - interfejs funkcji i funkcje natywne,
* Running/EnvironmentStack.cs - klasy związane z zarządzaniem zakresem zmiennych i samymi zmiennymi,
* SyntacticAnalysis/CommentSkippingLexer.cs - klasa opakowująca interfejs leksera w celu pominięcia komentarzy,
* Running/AstPrinter.cs - drukarz drzewa składniowego,
* klasy mapujące sekwencje znaków na tokeny, tokeny na operatory, literały, czy typy

## Testowanie

Analizator leksykalny oraz składniowy pracują na zasadzie konsumpcji kolejnych znaków (lub tokenów) i generowania rezultatu, który powinien być deterministyczny, a więc i możliwy do sprawdzenia pod kątem poprawności (np. poprzez proste porównanie sekwencji).

Komponenty systemu (obecnie skaner, lekser i parser) testowane są jednostkowo, niezależnie dzięki wykorzystaniu w implementacji podejścia obiektowego (możliwe będzie podstawienie obiektu np. leksera z użyciem atrapy).

Poza przykładami mającymi zadziałać poprawnie testowane są przypadki brzegowe, np. nagłe przerwanie strumienia wejściowego, nieprawidłowa sekwencja znaków/tokenów.

W testu interpretera zastosowane są testy integracyjne, podające na wejście tekst instrukcji i oczekujące określonego wyjścia.
