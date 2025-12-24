import java.io.IOException;
import java.lang.annotation.*;
import java.util.*;
import java.util.function.*;

// ==========================================
// 1. ANNOTATIONS
// Triggers: RuntimeVisibleAnnotations, AnnotationDefault
// ==========================================
@Retention(RetentionPolicy.RUNTIME)
@Target({ElementType.TYPE, ElementType.METHOD})
@interface TestMetadata {
    String author() default "AnvilASM"; // AnnotationDefault attribute
    int version();
}

// ==========================================
// 2. INTERFACES
// Triggers: InterfaceMethodref
// ==========================================
interface MathOperation {
    double execute(double a, double b);
}

// ==========================================
// 3. ENUMS
// Triggers: ACC_ENUM, Enum constant pool entries
// ==========================================
enum State {
    IDLE, PROCESSING, ERROR
}

// ==========================================
// 4. MAIN CLASS
// File name must be: Test.java
// ==========================================
@TestMetadata(version = 1)
public class Test<T> implements Cloneable {

    // ==========================================
    // FIELDS
    // Triggers: FieldInfo, ConstantValue, AccessFlags
    // ==========================================
    
    // ConstantValue attribute (Primitive)
    public static final int MAGIC_CONST = 42;
    
    // ConstantValue attribute (String)
    public static final String GREETING = "Hello Bytecode";
    
    // ACC_VOLATILE
    private volatile boolean isRunning;
    
    // ACC_TRANSIENT
    private transient Object tempCache;
    
    // Array type descriptor: [[[D
    private double[][][] complexArray;

    // ==========================================
    // CONSTRUCTORS
    // ==========================================
    public Test() {
        this.isRunning = true;
        this.complexArray = new double[2][2][2];
    }

    // ==========================================
    // METHODS
    // ==========================================

    /**
     * Test 1: Control Flow & Switch
     * Triggers: tableswitch, lookupswitch, StackMapTable
     */
    public String testControlFlow(int value) {
        // Compact sequence -> tableswitch
        switch (value) {
            case 0: return "Zero";
            case 1: return "One";
            case 2: return "Two";
        }

        // Sparse sequence -> lookupswitch
        switch (value) {
            case 100: return "Hundred";
            case 5000: return "Five Thousand";
            default: return "Unknown";
        }
    }

    /**
     * Test 2: Exception Handling
     * Triggers: ExceptionTable, Exceptions attribute (throws)
     */
    public void testExceptions() throws IOException, IllegalArgumentException {
        try {
            if (!isRunning) {
                throw new IOException("System not running");
            }
            // Multi-catch (Java 7+)
        } catch (IOException | NullPointerException e) {
            System.err.println("IO or Null Error: " + e.getMessage());
        } catch (Exception e) {
            System.err.println("General Error: " + e.getMessage());
        } finally {
            System.out.println("Cleanup executed");
        }
    }

    /**
     * Test 3: Lambdas & Dynamic Invocation
     * Triggers: invokedynamic instruction, BootstrapMethods attribute, MethodHandle
     */
    public void testLambdaAndStreams() {
        List<String> names = Arrays.asList("Alice", "Bob", "Charlie");

        // Lambda expression
        Consumer<String> printer = s -> System.out.println("Name: " + s);
        
        // Method Reference (System.out::println)
        names.forEach(System.out::println);
        
        // Stream pipeline
        long count = names.stream()
                .filter(s -> s.startsWith("A"))
                .count();
                
        System.out.println("Count: " + count);
    }

    /**
     * Test 4: Generics
     * Triggers: Signature attribute (LocalVariableTypeTable if -g is used)
     */
    public <E extends Number> void testGenerics(List<E> numbers, Map<String, E> map) {
        for (E num : numbers) {
            System.out.println("Number: " + num.doubleValue());
        }
    }

    /**
     * Test 5: Synchronization & Strict FP
     * Triggers: ACC_SYNCHRONIZED, ACC_STRICT, monitorenter/monitorexit
     */
    public strictfp synchronized void testSyncAndMath() {
        double result = 0.0;
        synchronized (this) {
            result = Math.PI * 2.0;
        }
        System.out.println("Strict Result: " + result);
    }

    /**
     * Test 6: Varargs & Native declaration
     * Triggers: ACC_VARARGS, ACC_NATIVE
     */
    public void testVarargs(String... args) {
        System.out.println("Args count: " + args.length);
    }

    // Native method declaration (no Code attribute)
    public native void nativeMethodExample();

    // ==========================================
    // INNER CLASSES
    // Triggers: InnerClasses attribute
    // ==========================================

    // 1. Static Nested Class
    public static class StaticNested {
        public int id;
    }

    // 2. Inner Member Class
    private class InnerMember {
        void accessOuter() {
            System.out.println(isRunning); // Accessing outer field
        }
    }

    public void testInnerClasses() {
        // 3. Local Class (Triggers EnclosingMethod attribute)
        class LocalClass {
            void print() { System.out.println("Local"); }
        }
        new LocalClass().print();

        // 4. Anonymous Class (Triggers EnclosingMethod attribute)
        Runnable r = new Runnable() {
            @Override
            public void run() {
                System.out.println("Anonymous");
            }
        };
        r.run();
    }

    // ==========================================
    // MAIN ENTRY POINT
    // ==========================================
    public static void main(String[] args) {
        System.out.println("=== Starting Bytecode Analysis Test ===");

        Test<Integer> test = new Test<>();

        // Run Control Flow
        System.out.println("Switch: " + test.testControlFlow(1));
        
        // Run Lambda
        test.testLambdaAndStreams();

        // Run Inner Classes
        test.testInnerClasses();
        
        // Run Exceptions
        try {
            test.testExceptions();
        } catch (Exception e) {
            // Ignore
        }

        System.out.println("=== Test Completed ===");
    }
}
