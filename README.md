# Toffee

Interpreter docelowo prostego i przyjemnego w użyciu języka skryptowego o roboczej nazwie Toffee.  
Język implementacji: C#.

Głównymi założeniami projektowanego języka są:
* wykorzystanie dynamicznego, słabego typowania,
* domyślna opcjonalność i mutowalność zmiennych,
* implementacja dopasowywania wzorca.

Przeważająca większość konstruktów językowych to wyrażenia, co ma uczynić pracę z językiem wygodniejszą.    
Obejmuje to m.in.:
* bloki kodu, które zwracają wartość ostatniego wyrażenia w nich zawartego (lub `null`, jeśli są puste),
* pętle `for` i `while` zwracające po zakończeniu wartość odpowiednio licznika oraz sprawdzanego warunku,
* przypisania wartości do zmiennych.

Język implementuje funkcje anonimowe i pozwala na przypisywanie ich do zmiennych.

Podstawowe typy danych będą przekazywane do funkcji przez kopię, natomiast łańcuchy znaków (których zawartość jest niezmienna) poprzez referencję.

Planowane jest wprowadzenie jakiejś struktury danych typu lista lub krotka (przykłady zamieszczone są w plikach tekstowych).


## Formalna specyfikacja i składnia

Gramatyka realizowanego języka opisana jest w pliku [gramatyka.md](gramatyka.md).

Nie przewiduje się na razie konfiguracji zachowania interpretera poprzez specjalne pliki.

Możliwe jest importowanie zawartości innych skryptów za pomocą instrukcji `pull`. Rolę biblioteki standardowej pełni przestrzeń nazw `std`. Zdefiniowane w "dociąganym" skrypcie elementy są wprowadzanie do przestrzeni nazw skryptu głównego, a w wypadku konfliktu nazw można odwołać się do nich z użyciem pełnej ścieżki (np. `std.io.print`).

## Wymagania niefunkcjonalne
1. Lekser i parser powinny działać na tyle szybko i sprawnie, by możliwe było wyświetlanie informacji np. o błędach składniowych na żywo w trakcie pisania programu.
2. Interpreter powinien być odporny na błędy - nie zawieszać się, nie przerywać nagle działania.

## Obsługa błędów

Dzięki śledzeniu pozycji w strumieniu, możliwe będzie logowanie użytkownikowi dokładnej pozycji błędu w linii.

W przypadku wystąpienia jakiegokolwiek błędu przerwanie bieżącego skryptu lub kawałka kodu zostanie przerwane (a dane zostaną bezpiecznie uprzątnięte).

## Sposób uruchomienia

Interpreter będzie dostarczony w postaci programu konsolowego. Uruchomiony bez argumentów, będzie pobierał dane ze standardowego strumienia wejściowego. Jako argumenty podać można natomiast pliki skryptowe, które zostaną wczytane i wykonane. W takim przypadku standardowe wejście pozostanie nieczynne. 

## Architektura

System składał się będzie z klas: analizatora leksykalnego, analizatora składniowego oraz interpretera wygenerowanego drzewa.

Przepływ informacji między systemami będzie jednokierunkowy - lekser pobierał będzie z wejścia kolejne znaki i podawał je analizatorowi składniowemu na żądanie. Ten z kolei będzie generował hierarchię obiektów nadającą się do wykonania przez interpreter.

Dla ułatwienia pracy zaimplementowane zostaną również klasy skanera (który będzie konwertował różne znaki nowej linii na jednolity format oraz śledził pozycję w strumieniu wejściowym) oraz potencjalnie różne klasy do obsługi błędów, logowania, itp.

## Testowanie

Analizator leksykalny oraz składniowy pracują na zasadzie konsumpcji kolejnych znaków (lub tokenów) i generowania rezultatu, który powinien być deterministyczny, a więc i możliwy do sprawdzenia pod kątem poprawności (np. poprzez proste porównanie sekwencji).

Komponenty systemu testowane będą jednostkowo, niezależnie dzięki wykorzystaniu podejścia obiektowego (możliwe podstawienie obiektu np. leksera z użyciem atrapy).

Oczywiście testowane będą, poza przykłądami mającymi zadziałać poprawnie, przypadki brzegowe, np. nagłe przerwanie strumienia wejściowego, dzielenie przez zero i inne.
